using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Raised when team details are updated
/// </summary>
public class TeamUpdatedEvent : DomainEventBase
{
    public string? OldName { get; init; }
    public string? NewName { get; init; }

    public TeamUpdatedEvent(
        TeamId teamId,
        string? oldName,
        string? newName,
        DateTime occurredAt)
    {
        OldName = oldName;
        NewName = newName;
    }
}