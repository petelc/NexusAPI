using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate;

/// <summary>
/// Represents a member of a team, linking a user to a team with specific roles and permissions
/// </summary>
public class TeamMember : EntityBase<TeamMemberId>
{
    public TeamId TeamId { get; private set; }
    public UserId UserId { get; private set; }
    public TeamRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; }

    private TeamMember() { }

    private TeamMember(TeamMemberId id, TeamId teamId, UserId userId, TeamRole role, DateTime joinedAt)
    {
        Id = Guard.Against.Null(id);
        TeamId = Guard.Against.Null(teamId);
        UserId = Guard.Against.Null(userId);
        Role = role;
        JoinedAt = joinedAt;
        IsActive = true;
    }

    public static TeamMember Create(TeamId teamId, UserId userId, TeamRole role)
    {
        Guard.Against.Null(teamId, nameof(teamId));
        Guard.Against.Null(userId, nameof(userId));

        return new TeamMember(TeamMemberId.CreateNew(), teamId, userId, role, DateTime.UtcNow);
    }

    public void ChangeRole(TeamRole newRole)
    {
        Role = newRole;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    public bool CanManageMembers() => Role == TeamRole.Owner || Role == TeamRole.Admin;
    public bool CanEditContent() => Role != TeamRole.Member;
    public bool IsOwner() => Role == TeamRole.Owner;
}