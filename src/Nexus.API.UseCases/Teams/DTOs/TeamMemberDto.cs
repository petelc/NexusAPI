namespace Nexus.API.UseCases.Teams.DTOs;

/// <summary>
/// Data transfer object for TeamMember
/// </summary>
public sealed class TeamMemberDto
{
    public Guid MemberId { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public int RoleValue { get; init; }
    public Guid? InvitedBy { get; init; }
    public DateTime JoinedAt { get; init; }
    public bool IsActive { get; init; }
}
