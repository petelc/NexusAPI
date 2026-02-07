using Ardalis.GuardClauses;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate;

/// <summary>
/// Represents a user's membership in a workspace
/// Entity within Workspace aggregate
/// </summary>
public class WorkspaceMember
{
  public WorkspaceMemberId Id { get; private set; }
  public UserId UserId { get; private set; }
  public WorkspaceMemberRole Role { get; private set; }
  public DateTime JoinedAt { get; private set; }
  public UserId? InvitedBy { get; private set; }
  public bool IsActive { get; private set; }

  // EF Core constructor
  private WorkspaceMember() { }

  private WorkspaceMember(
    WorkspaceMemberId id,
    UserId userId,
    WorkspaceMemberRole role,
    UserId? invitedBy,
    DateTime joinedAt)
  {
    Id = Guard.Against.Null(id);
    UserId = Guard.Against.Null(userId);
    Role = role;
    InvitedBy = invitedBy;
    JoinedAt = joinedAt;
    IsActive = true;
  }

  public static WorkspaceMember Create(
    UserId userId,
    WorkspaceMemberRole role,
    UserId? invitedBy = null)
  {
    return new WorkspaceMember(
      WorkspaceMemberId.CreateNew(),
      userId,
      role,
      invitedBy,
      DateTime.UtcNow);
  }

  public void ChangeRole(WorkspaceMemberRole newRole)
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

  public bool CanManageMembers() => Role == WorkspaceMemberRole.Owner || Role == WorkspaceMemberRole.Admin;
  public bool CanEditContent() => Role != WorkspaceMemberRole.Viewer;
  public bool IsOwner() => Role == WorkspaceMemberRole.Owner;
}
