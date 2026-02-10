using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Aggregates.CollaborationAggregate;

/// <summary>
/// CollaborationSession aggregate root
/// Represents a real-time collaboration session on a document or diagram
/// </summary>
public class CollaborationSession : EntityBase<SessionId>, IAggregateRoot
{
    // Properties
    public ResourceType ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<SessionParticipant> _participants = new();
    public IReadOnlyCollection<SessionParticipant> Participants => _participants.AsReadOnly();

    private readonly List<SessionChange> _changes = new();
    public IReadOnlyCollection<SessionChange> Changes => _changes.AsReadOnly();

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    // Private constructor for EF Core
    private CollaborationSession() { }

    /// <summary>
    /// Factory method to start a new collaboration session
    /// </summary>
    public static CollaborationSession Start(
        ResourceType resourceType,
        Guid resourceId,
        Guid initiatorUserId)
    {
        var session = new CollaborationSession
        {
            Id = SessionId.CreateNew(),
            ResourceType = resourceType,
            ResourceId = resourceId,
            StartedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add initiator as first participant with Editor role
        session.AddParticipant(initiatorUserId, ParticipantRole.Editor);

        return session;
    }

    /// <summary>
    /// Add a participant to the session
    /// </summary>
    public void AddParticipant(Guid userId, ParticipantRole role)
    {
        // Check if user is already an active participant
        if (_participants.Any(p => p.UserId == userId && !p.LeftAt.HasValue))
        {
            throw new InvalidOperationException("User is already an active participant in this session");
        }

        var participant = SessionParticipant.Create(Id, userId, role);
        _participants.Add(participant);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a participant from the session
    /// </summary>
    public void RemoveParticipant(ParticipantId userId)
    {
        var participant = _participants.FirstOrDefault(
            p => p.UserId == userId && !p.LeftAt.HasValue);

        if (participant == null)
        {
            throw new InvalidOperationException("User is not an active participant");
        }

        participant.Leave();
        UpdatedAt = DateTime.UtcNow;

        // End session if no active participants remain
        if (!_participants.Any(p => !p.LeftAt.HasValue))
        {
            End();
        }
    }

    /// <summary>
    /// Apply a change to the session
    /// </summary>
    public void ApplyChange(
        ParticipantId userId,
        ChangeType changeType,
        int position,
        string data)
    {
        // Verify user is an active participant with editor role
        var participant = _participants.FirstOrDefault(
            p => p.UserId == userId && !p.LeftAt.HasValue);

        if (participant == null)
        {
            throw new InvalidOperationException("User is not an active participant");
        }

        if (participant.Role != ParticipantRole.Editor)
        {
            throw new InvalidOperationException("User does not have editor permissions");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Session is not active");
        }

        var change = SessionChange.Create(Id, userId, changeType, position, data);
        _changes.Add(change);
        participant.UpdateLastActivity();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a comment to the session
    /// </summary>
    public void AddComment(
        ParticipantId userId,
        string text,
        int? position = null)
    {
        // Verify user is an active participant
        if (!_participants.Any(p => p.UserId == userId && !p.LeftAt.HasValue))
        {
            throw new InvalidOperationException("User is not an active participant");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Session is not active");
        }

        var comment = Comment.Create(Id.Value, ResourceType, ResourceId, userId, text, position);
        _comments.Add(comment);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update cursor position for a participant
    /// </summary>
    public void UpdateCursorPosition(ParticipantId userId, int? cursorPosition)
    {
        var participant = _participants.FirstOrDefault(
            p => p.UserId == userId && !p.LeftAt.HasValue);

        if (participant == null)
        {
            throw new InvalidOperationException("User is not an active participant");
        }

        participant.UpdateCursorPosition(cursorPosition);
        participant.UpdateLastActivity();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// End the collaboration session
    /// </summary>
    public void End()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Session is already ended");
        }

        EndedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // Mark all active participants as left
        foreach (var participant in _participants.Where(p => !p.LeftAt.HasValue))
        {
            participant.Leave();
        }
    }

    /// <summary>
    /// Get active participants count
    /// </summary>
    public int GetActiveParticipantCount()
    {
        return _participants.Count(p => !p.LeftAt.HasValue);
    }

    /// <summary>
    /// Check if user is an active participant
    /// </summary>
    public bool IsUserActive(ParticipantId userId)
    {
        return _participants.Any(p => p.UserId == userId && !p.LeftAt.HasValue);
    }

    public bool IsUserActiveParticipant(ParticipantId userId)
    {
        return _participants.Any(p => p.UserId == userId && p.IsActive);
    }

    /// <summary>
    /// Get participant by user ID
    /// </summary>
    public SessionParticipant? GetParticipant(ParticipantId userId)
    {
        return _participants.FirstOrDefault(p => p.UserId == userId && !p.LeftAt.HasValue);
    }
}
