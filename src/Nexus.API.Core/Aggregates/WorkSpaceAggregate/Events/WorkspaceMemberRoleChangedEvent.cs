using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a member's role is changed
/// </summary>
public class WorkspaceMemberRoleChangedEvent : DomainEventBase
{
  public WorkspaceId WorkspaceId { get; }
  public UserId UserId { get; }
  public WorkspaceMemberRole NewRole { get; }

  public WorkspaceMemberRoleChangedEvent(WorkspaceId workspaceId, UserId userId, WorkspaceMemberRole newRole)
  {
    WorkspaceId = workspaceId;
    UserId = userId;
    NewRole = newRole;
  }
}