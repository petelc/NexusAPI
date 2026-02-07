using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a member is removed from a workspace
/// </summary>
public class WorkspaceMemberRemovedEvent : DomainEventBase
{
  public WorkspaceId WorkspaceId { get; }
  public UserId UserId { get; }

  public WorkspaceMemberRemovedEvent(WorkspaceId workspaceId, UserId userId)
  {
    WorkspaceId = workspaceId;
    UserId = userId;
  }
}