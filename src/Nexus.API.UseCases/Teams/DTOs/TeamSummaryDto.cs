namespace Nexus.API.UseCases.Teams.DTOs;

/// <summary>
/// Lightweight DTO for team list views
/// </summary>
public sealed class TeamSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int MemberCount { get; init; }
    public string? UserRole { get; init; }
}
