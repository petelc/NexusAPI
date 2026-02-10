namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response DTO for session change
/// </summary>
public record SessionChangeResponseDto
{
    public Guid ChangeId { get; init; }
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string ChangeType { get; init; } = string.Empty;
    public int Position { get; init; }
    public string? Data { get; init; }
}