using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Raised when a new team is created
/// </summary>
public class TeamCreatedEvent : DomainEventBase
{
    public TeamId TeamId { get; init; }
    public string Name { get; init; }
    public Guid CreatedBy { get; init; }

    public TeamCreatedEvent(
        TeamId teamId,
        string name,
        Guid createdBy,
        DateTime occurredAt)
    {
        Name = name;
        CreatedBy = createdBy;
    }
}