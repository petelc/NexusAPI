using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

/// <summary>
/// Domain event raised when a workspace is deleted
/// </summary>
public class WorkspaceDeletedEvent : DomainEventBase
{
    public WorkspaceId WorkspaceId { get; }

    public WorkspaceDeletedEvent(WorkspaceId workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}