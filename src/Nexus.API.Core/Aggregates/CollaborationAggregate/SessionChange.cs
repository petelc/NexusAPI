using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.CollaborationAggregate;

/// <summary>
/// SessionChange entity
/// Represents a change made during a collaboration session
/// </summary>
public class SessionChange : EntityBase<ChangeId>
{
    // Properties
    public SessionId SessionId { get; private set; }
    public ParticipantId UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime Timestamp { get; private set; }
    public ChangeType ChangeType { get; private set; }
    public int Position { get; private set; }
    public string? Data { get; private set; }
    public byte[]? ChangeHash { get; private set; }

    // Navigation properties
    public CollaborationSession Session { get; private set; } = null!;

    // Private constructor for EF Core
    private SessionChange() { }

    /// <summary>
    /// Factory method to create a new change
    /// </summary>
    public static SessionChange Create(
        SessionId sessionId,
        ParticipantId userId,
        ChangeType changeType,
        int position,
        string? data)
    {
        var change = new SessionChange
        {
            Id = ChangeId.CreateNew(),
            SessionId = sessionId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            ChangeType = changeType,
            Position = position,
            Data = data,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Generate hash for conflict detection
        change.ChangeHash = change.GenerateChangeHash();

        return change;
    }

    /// <summary>
    /// Generate hash for change data (for conflict detection)
    /// </summary>
    private byte[] GenerateChangeHash()
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashInput = $"{SessionId.Value}|{UserId.Value}|{Timestamp:O}|{ChangeType}|{Position}|{Data}";
        return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashInput));
    }
}
