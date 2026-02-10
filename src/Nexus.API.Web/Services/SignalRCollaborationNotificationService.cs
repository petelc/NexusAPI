using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.Web.Services;

/// <summary>
/// SignalR implementation of ICollaborationNotificationService.
/// Lives in the Web layer - has access to IHubContext.
/// Registered in DI so UseCases handlers receive this via ICollaborationNotificationService.
/// </summary>
public class SignalRCollaborationNotificationService : ICollaborationNotificationService
{
    private readonly IHubContext<Hubs.CollaborationHub> _hubContext;
    private readonly ILogger<SignalRCollaborationNotificationService> _logger;

    public SignalRCollaborationNotificationService(
        IHubContext<Hubs.CollaborationHub> hubContext,
        ILogger<SignalRCollaborationNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── Session Events ───────────────────────────────────────────────────────

    public async Task NotifySessionStartedAsync(
        Guid sessionId,
        Guid resourceId,
        string resourceType,
        Guid startedByUserId,
        DateTime startedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resourceGroup = GetResourceGroup(resourceType, resourceId);

            await _hubContext.Clients.Group(resourceGroup).SendAsync(
                "SessionStarted",
                new
                {
                    SessionId = sessionId,
                    ResourceType = resourceType,
                    ResourceId = resourceId,
                    StartedBy = startedByUserId,
                    StartedAt = startedAt,
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken);

            _logger.LogDebug(
                "Broadcasted SessionStarted for session {SessionId} on {ResourceType} {ResourceId}",
                sessionId, resourceType, resourceId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast SessionStarted for session {SessionId}",
                sessionId);
        }
    }

    public async Task NotifySessionEndedAsync(
        Guid sessionId,
        Guid resourceId,
        string resourceType,
        Guid endedByUserId,
        DateTime endedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                SessionId = sessionId,
                ResourceType = resourceType,
                ResourceId = resourceId,
                EndedBy = endedByUserId,
                EndedAt = endedAt,
                Timestamp = DateTime.UtcNow
            };

            // Notify active session participants
            var sessionGroup = GetSessionGroup(sessionId);
            await _hubContext.Clients.Group(sessionGroup)
                .SendAsync("SessionEnded", payload, cancellationToken);

            // Notify resource watchers
            var resourceGroup = GetResourceGroup(resourceType, resourceId);
            await _hubContext.Clients.Group(resourceGroup)
                .SendAsync("SessionEnded", payload, cancellationToken);

            _logger.LogDebug(
                "Broadcasted SessionEnded for session {SessionId}",
                sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast SessionEnded for session {SessionId}",
                sessionId);
        }
    }

    public async Task NotifyParticipantAddedAsync(
        Guid sessionId,
        Guid userId,
        string role,
        int activeParticipantCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionGroup = GetSessionGroup(sessionId);

            await _hubContext.Clients.Group(sessionGroup).SendAsync(
                "ParticipantAdded",
                new ParticipantInfoDto
                {
                    UserId = userId,
                    Username = string.Empty, // TODO: resolve from IUserService
                    FullName = string.Empty,
                    Role = role,
                    JoinedAt = DateTime.UtcNow
                },
                cancellationToken);

            await _hubContext.Clients.Group(sessionGroup).SendAsync(
                "SessionStatusChanged",
                new SessionStatusDto
                {
                    SessionId = sessionId,
                    Status = "Active",
                    ParticipantCount = activeParticipantCount,
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken);

            _logger.LogDebug(
                "Broadcasted ParticipantAdded for user {UserId} in session {SessionId}",
                userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast ParticipantAdded for session {SessionId}",
                sessionId);
        }
    }

    public async Task NotifyParticipantRemovedAsync(
        Guid sessionId,
        Guid userId,
        int activeParticipantCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionGroup = GetSessionGroup(sessionId);

            await _hubContext.Clients.Group(sessionGroup).SendAsync(
                "ParticipantRemoved",
                new ParticipantInfoDto
                {
                    UserId = userId,
                    Username = string.Empty,
                    FullName = string.Empty,
                    Role = string.Empty,
                    JoinedAt = DateTime.UtcNow
                },
                cancellationToken);

            await _hubContext.Clients.Group(sessionGroup).SendAsync(
                "SessionStatusChanged",
                new SessionStatusDto
                {
                    SessionId = sessionId,
                    Status = activeParticipantCount > 0 ? "Active" : "Inactive",
                    ParticipantCount = activeParticipantCount,
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken);

            _logger.LogDebug(
                "Broadcasted ParticipantRemoved for user {UserId} in session {SessionId}",
                userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast ParticipantRemoved for session {SessionId}",
                sessionId);
        }
    }

    // ─── Comment Events ───────────────────────────────────────────────────────

    public async Task NotifyCommentAddedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionId.HasValue)
            {
                await _hubContext.Clients.Group(GetSessionGroup(sessionId.Value))
                    .SendAsync("CommentAdded", comment, cancellationToken);
            }

            var resourceGroup = GetResourceGroup(comment.ResourceType, comment.ResourceId);
            await _hubContext.Clients.Group(resourceGroup)
                .SendAsync("CommentAdded", comment, cancellationToken);

            _logger.LogDebug(
                "Broadcasted CommentAdded for comment {CommentId} on {ResourceType} {ResourceId}",
                comment.CommentId, comment.ResourceType, comment.ResourceId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast CommentAdded for comment {CommentId}",
                comment.CommentId);
        }
    }

    public async Task NotifyCommentUpdatedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionId.HasValue)
            {
                await _hubContext.Clients.Group(GetSessionGroup(sessionId.Value))
                    .SendAsync("CommentUpdated", comment, cancellationToken);
            }

            var resourceGroup = GetResourceGroup(comment.ResourceType, comment.ResourceId);
            await _hubContext.Clients.Group(resourceGroup)
                .SendAsync("CommentUpdated", comment, cancellationToken);

            _logger.LogDebug(
                "Broadcasted CommentUpdated for comment {CommentId}",
                comment.CommentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast CommentUpdated for comment {CommentId}",
                comment.CommentId);
        }
    }

    public async Task NotifyCommentDeletedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionId.HasValue)
            {
                await _hubContext.Clients.Group(GetSessionGroup(sessionId.Value))
                    .SendAsync("CommentDeleted", comment, cancellationToken);
            }

            var resourceGroup = GetResourceGroup(comment.ResourceType, comment.ResourceId);
            await _hubContext.Clients.Group(resourceGroup)
                .SendAsync("CommentDeleted", comment, cancellationToken);

            _logger.LogDebug(
                "Broadcasted CommentDeleted for comment {CommentId}",
                comment.CommentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast CommentDeleted for comment {CommentId}",
                comment.CommentId);
        }
    }

    public async Task NotifyReplyReceivedAsync(
        Guid parentCommentAuthorUserId,
        Guid replyCommentId,
        Guid parentCommentId,
        Guid resourceId,
        string resourceType,
        Guid replyAuthorUserId,
        string replyText,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Only notify if the reply is from a different user
            if (parentCommentAuthorUserId == replyAuthorUserId)
                return;

            var userGroup = GetUserGroup(parentCommentAuthorUserId);

            await _hubContext.Clients.Group(userGroup).SendAsync(
                "ReplyReceived",
                new
                {
                    ReplyCommentId = replyCommentId,
                    ParentCommentId = parentCommentId,
                    ResourceId = resourceId,
                    ResourceType = resourceType,
                    FromUserId = replyAuthorUserId,
                    FromUsername = string.Empty, // TODO: resolve from IUserService
                    Text = replyText,
                    CreatedAt = createdAt
                },
                cancellationToken);

            _logger.LogDebug(
                "Broadcasted ReplyReceived to user {UserId} for comment {CommentId}",
                parentCommentAuthorUserId, parentCommentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to broadcast ReplyReceived to user {UserId}",
                parentCommentAuthorUserId);
        }
    }

    // ─── Group Name Helpers ───────────────────────────────────────────────────

    private static string GetSessionGroup(Guid sessionId)
        => sessionId.ToString();

    private static string GetResourceGroup(string resourceType, Guid resourceId)
        => $"{resourceType}_{resourceId}";

    private static string GetUserGroup(Guid userId)
        => $"user_{userId}";
}
