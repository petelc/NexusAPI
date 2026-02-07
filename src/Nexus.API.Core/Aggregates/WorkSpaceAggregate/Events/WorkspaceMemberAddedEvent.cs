using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a member is added to a workspace
/// </summary>
public class WorkspaceMemberAddedEvent : DomainEventBase
{
  public WorkspaceId WorkspaceId { get; }
  public UserId UserId { get; }
  public WorkspaceMemberRole Role { get; }

  public WorkspaceMemberAddedEvent(WorkspaceId workspaceId, UserId userId, WorkspaceMemberRole role)
  {
    WorkspaceId = workspaceId;
    UserId = userId;
    Role = role;
  }
}