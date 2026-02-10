namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response for active sessions
/// </summary>
public record ActiveSessionsResponseDto
{
    public List<CollaborationSessionResponseDto> Sessions { get; init; } = new();
    public int TotalCount { get; init; }
}