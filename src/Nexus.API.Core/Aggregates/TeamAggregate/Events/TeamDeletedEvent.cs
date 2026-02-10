
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Raised when a team is deleted (soft delete)
/// </summary>
public class TeamDeletedEvent : DomainEventBase
{
    public Guid DeletedBy { get; init; }

    public TeamDeletedEvent(
        TeamId teamId,
        Guid deletedBy,
        DateTime occurredAt)
    {
        DeletedBy = deletedBy;
    }
}