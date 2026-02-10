using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;

/// <summary>
/// Raised when a member is added to the team
/// </summary>
public class TeamMemberAddedEvent : DomainEventBase
{
    public TeamMemberId MemberId { get; init; }
    public Guid UserId { get; init; }
    public TeamRole Role { get; init; }
    public Guid? InvitedBy { get; init; }

    public TeamMemberAddedEvent(
        TeamId teamId,
        TeamMemberId memberId,
        Guid userId,
        TeamRole role,
        Guid? invitedBy,
        DateTime occurredAt)
    {
        MemberId = memberId;
        UserId = userId;
        Role = role;
        InvitedBy = invitedBy;
    }
}