using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.Web.Hubs;

/// <summary>
/// SignalR Hub for real-time collaboration features
/// Handles session management, live updates, cursor tracking, and notifications
/// </summary>
[Authorize]
public class CollaborationHub : Hub
{
    private readonly IConnectionManager _connectionManager;
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CollaborationHub> _logger;

    public CollaborationHub(
        IConnectionManager connectionManager,
        ICollaborationRepository collaborationRepository,
        IUserRepository userRepository,
        ILogger<CollaborationHub> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Connection Lifecycle

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirstValue("name") ?? "Unknown";

        _logger.LogInformation("User {UserId} ({Username}) connected: {ConnectionId}",
            userId, username, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var sessionId = _connectionManager.GetConnectionSession(Context.ConnectionId);

        if (sessionId.HasValue)
        {
            _logger.LogInformation("User {UserId} disconnected from session {SessionId}: {ConnectionId}",
                userId, sessionId.Value, Context.ConnectionId);

            // Remove from session
            await _connectionManager.RemoveFromSessionAsync(Context.ConnectionId, sessionId.Value);

            // Check if user has any other connections in this session
            if (!_connectionManager.IsUserConnected(sessionId.Value, userId))
            {
                // Notify others that user left
                var username = Context.User?.FindFirstValue("name") ?? "Unknown";
                await Clients.Group(sessionId.Value.ToString())
                    .SendAsync("ParticipantLeft", new ParticipantInfoDto
                    {
                        UserId = userId,
                        Username = username,
                        FullName = Context.User?.FindFirstValue("name") ?? username,
                        Role = "Unknown",
                        JoinedAt = DateTime.UtcNow
                    });

                // Update session status
                var participantCount = _connectionManager.GetSessionUsers(sessionId.Value).Count;
                await Clients.Group(sessionId.Value.ToString())
                    .SendAsync("SessionStatusChanged", new SessionStatusDto
                    {
                        SessionId = sessionId.Value,
                        Status = participantCount > 0 ? "Active" : "Inactive",
                        ParticipantCount = participantCount,
                        Timestamp = DateTime.UtcNow
                    });
            }
        }

        await _connectionManager.CleanupConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Join a collaboration session
    /// </summary>
    public async Task JoinSession(SessionId sessionId)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirstValue("name") ?? "Unknown";
        var fullName = Context.User?.FindFirstValue("name") ?? username;

        _logger.LogInformation("User {UserId} joining session {SessionId}", userId, sessionId);

        // Verify session exists and user has access
        var session = await _collaborationRepository.GetSessionByIdAsync(sessionId, CancellationToken.None);
        if (session == null)
        {
            throw new HubException("Session not found");
        }

        if (!session.IsUserActiveParticipant(ParticipantId.Create(userId)))
        {
            throw new HubException("User is not a participant in this session");
        }

        // Add to SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

        // Track connection
        await _connectionManager.AddToSessionAsync(Context.ConnectionId, sessionId, userId);

        // Get participant info
        var participant = session.Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);
        var participantInfo = new ParticipantInfoDto
        {
            UserId = userId,
            Username = username,
            FullName = fullName,
            Role = participant?.Role.ToString() ?? "Viewer",
            JoinedAt = participant?.JoinedAt ?? DateTime.UtcNow
        };

        // Notify others in the session
        await Clients.OthersInGroup(sessionId.ToString())
            .SendAsync("ParticipantJoined", participantInfo);

        // Send current session state to joining user
        // Look up all active participant user info
        var activeParticipants = session.Participants
            .Where(p => p.IsActive && _connectionManager.IsUserConnected(sessionId, p.UserId))
            .ToList();
        var participantUserIds = activeParticipants.Select(p => p.UserId).Distinct().ToList();
        var userLookup = new Dictionary<Guid, Core.Aggregates.UserAggregate.User>();
        foreach (var uid in participantUserIds)
        {
            var user = await _userRepository.GetByIdAsync(UserId.From(uid));
            if (user != null)
                userLookup[uid] = user;
        }

        var allParticipants = activeParticipants
            .Select(p =>
            {
                userLookup.TryGetValue(p.UserId, out var pUser);
                return new ParticipantInfoDto
                {
                    UserId = p.UserId,
                    Username = pUser?.Username ?? "Unknown",
                    FullName = pUser != null ? $"{pUser.FullName.FirstName} {pUser.FullName.LastName}" : "Unknown User",
                    Role = p.Role.ToString(),
                    JoinedAt = p.JoinedAt
                };
            })
            .ToList();

        var cursorPositions = _connectionManager.GetAllCursorPositions(sessionId)
            .Select(kvp =>
            {
                userLookup.TryGetValue(kvp.Key, out var cUser);
                return new CursorPositionDto
                {
                    UserId = kvp.Key,
                    Username = cUser?.Username ?? "Unknown",
                    Position = kvp.Value,
                    Timestamp = DateTime.UtcNow
                };
            })
            .ToList();

        await Clients.Caller.SendAsync("SessionSynced", new SessionSyncDto
        {
            SessionId = sessionId,
            ActiveParticipants = allParticipants,
            CursorPositions = cursorPositions,
            SyncTimestamp = DateTime.UtcNow
        });

        // Update session status
        var participantCount = _connectionManager.GetSessionUsers(sessionId).Count;
        await Clients.Group(sessionId.ToString())
            .SendAsync("SessionStatusChanged", new SessionStatusDto
            {
                SessionId = sessionId,
                Status = "Active",
                ParticipantCount = participantCount,
                Timestamp = DateTime.UtcNow
            });

        _logger.LogInformation("User {UserId} successfully joined session {SessionId}", userId, sessionId);
    }

    /// <summary>
    /// Leave a collaboration session
    /// </summary>
    public async Task LeaveSession(SessionId sessionId)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirstValue("name") ?? "Unknown";

        _logger.LogInformation("User {UserId} leaving session {SessionId}", userId, sessionId);

        // Remove from SignalR group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId.ToString());

        // Remove connection tracking
        await _connectionManager.RemoveFromSessionAsync(Context.ConnectionId, sessionId);

        // Check if user has any other connections in this session
        if (!_connectionManager.IsUserConnected(sessionId, userId))
        {
            // Notify others that user left
            await Clients.Group(sessionId.ToString())
                .SendAsync("ParticipantLeft", new ParticipantInfoDto
                {
                    UserId = userId,
                    Username = username,
                    FullName = Context.User?.FindFirstValue("name") ?? username,
                    Role = "Unknown",
                    JoinedAt = DateTime.UtcNow
                });

            // Update session status
            var participantCount = _connectionManager.GetSessionUsers(sessionId).Count;
            await Clients.Group(sessionId.ToString())
                .SendAsync("SessionStatusChanged", new SessionStatusDto
                {
                    SessionId = sessionId,
                    Status = participantCount > 0 ? "Active" : "Inactive",
                    ParticipantCount = participantCount,
                    Timestamp = DateTime.UtcNow
                });
        }

        _logger.LogInformation("User {UserId} successfully left session {SessionId}", userId, sessionId);
    }

    #endregion

    #region Cursor & Typing

    /// <summary>
    /// Update cursor position in the document/diagram
    /// </summary>
    public async Task UpdateCursorPosition(SessionId sessionId, int position)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirstValue("name") ?? "Unknown";

        // Update in connection manager
        _connectionManager.UpdateCursorPosition(sessionId, userId, position);

        // Broadcast to others
        await Clients.OthersInGroup(sessionId.ToString())
            .SendAsync("CursorMoved", new CursorPositionDto
            {
                UserId = userId,
                Username = username,
                Position = position,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Update typing status
    /// </summary>
    public async Task NotifyTyping(SessionId sessionId, bool isTyping)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirstValue("name") ?? "Unknown";

        // Update in connection manager
        _connectionManager.UpdateTypingStatus(sessionId, userId, isTyping);

        // Broadcast to others
        await Clients.OthersInGroup(sessionId.ToString())
            .SendAsync("TypingStatusChanged", new TypingStatusDto
            {
                UserId = userId,
                Username = username,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
    }

    #endregion

    #region Changes

    /// <summary>
    /// Send a change to all participants in the session
    /// </summary>
    public async Task SendChange(SessionId sessionId, ChangeDto change)
    {
        var userId = GetUserId();

        // Validate user is in session
        if (!_connectionManager.IsUserConnected(sessionId, userId))
        {
            throw new HubException("User is not connected to this session");
        }

        // Broadcast change to others
        await Clients.OthersInGroup(sessionId.ToString())
            .SendAsync("ChangeReceived", change);

        _logger.LogDebug("Change {ChangeId} sent by user {UserId} in session {SessionId}",
            change.ChangeId, userId, sessionId);
    }

    #endregion

    #region Helper Methods

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirstValue("uid")
            ?? Context.User?.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("User ID not found in token");
        }

        return userId;
    }

    #endregion
}
