using Nexus.API.Core.Exceptions;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;


namespace Nexus.API.Core.Aggregates.TeamAggregate;

/// <summary>
/// Represents a member of a team
/// Links a user to a team with specific roles and permissions
/// </summary>
public class TeamMember : EntityBase<TeamMemberId>
{
    public TeamId TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public TeamRole Role { get; private set; }
    public Guid? InvitedBy { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core constructor
    private TeamMember() { }

    private TeamMember(
        TeamMemberId id,
        TeamId teamId,
        Guid userId,
        TeamRole role,
        Guid? invitedBy,
        DateTime joinedAt)
    {
        Id = Guard.Against.Null(id);
        TeamId = Guard.Against.Null(teamId);
        UserId = Guard.Against.Null(userId);
        Role = role;
        InvitedBy = invitedBy;
        JoinedAt = joinedAt;
        IsActive = true;
    }

    /// <summary>
    /// Factory method to create a new team member
    /// </summary>
    public static TeamMember Create(
        TeamId teamId,
        Guid userId,
        TeamRole role,
        Guid? invitedBy = null)
    {
        Guard.Against.Null(teamId, nameof(teamId));
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        return new TeamMember(
            TeamMemberId.CreateNew(),
            teamId,
            userId,
            role,
            invitedBy,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Change the member's role
    /// </summary>
    public void ChangeRole(TeamRole newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
    }

    /// <summary>
    /// Deactivate the member (soft remove)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate the member
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Check if member can manage other members
    /// </summary>
    public bool CanManageMembers() => Role == TeamRole.Owner || Role == TeamRole.Admin;

    /// <summary>
    /// Check if member can edit team content
    /// </summary>
    public bool CanEditContent() => Role != TeamRole.Member;

    /// <summary>
    /// Check if member is an owner
    /// </summary>
    public bool IsOwner() => Role == TeamRole.Owner;
}
