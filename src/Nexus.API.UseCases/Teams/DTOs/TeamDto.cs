namespace Nexus.API.UseCases.Teams.DTOs;

/// <summary>
/// Data transfer object for Team
/// </summary>
public sealed class TeamDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<TeamMemberDto> Members { get; init; } = new();
    public int MemberCount { get; init; }
    public int OwnerCount { get; init; }
}
