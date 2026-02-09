namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response DTO for collaboration session
/// </summary>
public record CollaborationSessionResponseDto
{
    public Guid SessionId { get; init; }
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public bool IsActive { get; init; }
    public int ActiveParticipantCount { get; init; }
    public List<SessionParticipantDto> Participants { get; init; } = new();
}