using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Base class for Team domain events
/// </summary>
public abstract class TeamDomainEvent
{
    public TeamId TeamId { get; init; }
    public DateTime OccurredAt { get; init; }

    protected TeamDomainEvent(TeamId teamId, DateTime occurredAt)
    {
        TeamId = teamId;
        OccurredAt = occurredAt;
    }
}