using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Raised when a member is removed from the team
/// </summary>
public class TeamMemberRemovedEvent : DomainEventBase
{
    public TeamMemberId MemberId { get; init; }
    public Guid UserId { get; init; }

    public TeamMemberRemovedEvent(
        TeamId teamId,
        TeamMemberId memberId,
        Guid userId,
        DateTime occurredAt)
    {
        MemberId = memberId;
        UserId = userId;
    }
}