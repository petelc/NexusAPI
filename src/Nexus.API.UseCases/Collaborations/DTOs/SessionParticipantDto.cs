namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Session participant DTO
/// </summary>
public record SessionParticipantDto
{
    public Guid ParticipantId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime? LeftAt { get; init; }
    public DateTime? LastActivityAt { get; init; }
    public int? CursorPosition { get; init; }
    public bool IsActive { get; init; }
}