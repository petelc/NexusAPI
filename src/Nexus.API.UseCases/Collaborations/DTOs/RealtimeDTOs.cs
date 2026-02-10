namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// DTO for participant information in real-time events
/// </summary>
public record ParticipantInfoDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty; // "Viewer" or "Editor"
    public DateTime JoinedAt { get; init; }
}

/// <summary>
/// DTO for cursor position updates
/// </summary>
public record CursorPositionDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public int Position { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// DTO for typing indicator
/// </summary>
public record TypingStatusDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public bool IsTyping { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// DTO for document/diagram change
/// </summary>
public record ChangeDto
{
    public Guid ChangeId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string ChangeType { get; init; } = string.Empty; // "Insert", "Update", "Delete"
    public int Position { get; init; }
    public string? Data { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// DTO for session status update
/// </summary>
public record SessionStatusDto
{
    public Guid SessionId { get; init; }
    public string Status { get; init; } = string.Empty; // "Active", "Ended"
    public int ParticipantCount { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// DTO for real-time comment notification
/// </summary>
public record CommentNotificationDto
{
    public Guid CommentId { get; init; }
    public Guid ResourceId { get; init; }
    public string ResourceType { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int? Position { get; init; }
    public Guid? ParentCommentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Action { get; init; } = string.Empty; // "Added", "Updated", "Deleted"
}

/// <summary>
/// DTO for presence information
/// </summary>
public record PresenceDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // "Active", "Idle", "Away"
    public DateTime LastActivityAt { get; init; }
}

/// <summary>
/// DTO for session sync (when reconnecting)
/// </summary>
public record SessionSyncDto
{
    public Guid SessionId { get; init; }
    public List<ParticipantInfoDto> ActiveParticipants { get; init; } = new();
    public List<CursorPositionDto> CursorPositions { get; init; } = new();
    public DateTime SyncTimestamp { get; init; }
}
