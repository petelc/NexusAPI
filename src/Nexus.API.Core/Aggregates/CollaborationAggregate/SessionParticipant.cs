using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.CollaborationAggregate;

/// <summary>
/// SessionParticipant entity
/// Represents a user participating in a collaboration session
/// </summary>
public class SessionParticipant : EntityBase<ParticipantId>
{
    // Properties
    public SessionId SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public int? CursorPosition { get; private set; }

    // Navigation properties
    public CollaborationSession Session { get; private set; } = null!;

    // Private constructor for EF Core
    private SessionParticipant() { }

    /// <summary>
    /// Factory method to create a new participant
    /// </summary>
    public static SessionParticipant Create(
        SessionId sessionId,
        Guid userId,
        ParticipantRole role)
    {
        return new SessionParticipant
        {
            Id = ParticipantId.CreateNew(),
            SessionId = sessionId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mark participant as having left the session
    /// </summary>
    public void Leave()
    {
        if (LeftAt.HasValue)
        {
            throw new InvalidOperationException("Participant has already left");
        }

        LeftAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update cursor position
    /// </summary>
    public void UpdateCursorPosition(int? position)
    {
        CursorPosition = position;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update last activity timestamp
    /// </summary>
    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if participant is still active
    /// </summary>
    public bool IsActive => !LeftAt.HasValue;
}
