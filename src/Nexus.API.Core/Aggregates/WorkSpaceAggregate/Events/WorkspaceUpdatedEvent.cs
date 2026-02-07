using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a workspace is updated
/// </summary>
public class WorkspaceUpdatedEvent : DomainEventBase
{
    public WorkspaceId WorkspaceId { get; }

    public WorkspaceUpdatedEvent(WorkspaceId workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}