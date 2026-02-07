using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a workspace is created
/// </summary>
public class WorkspaceCreatedEvent : DomainEventBase
{
  public WorkspaceId WorkspaceId { get; }
  public TeamId TeamId { get; }
  public UserId CreatedBy { get; }

  public WorkspaceCreatedEvent(WorkspaceId workspaceId, TeamId teamId, UserId createdBy)
  {
    WorkspaceId = workspaceId;
    TeamId = teamId;
    CreatedBy = createdBy;
  }
}