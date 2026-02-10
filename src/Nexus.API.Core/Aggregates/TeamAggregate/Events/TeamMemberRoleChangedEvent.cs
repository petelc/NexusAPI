using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate.Events;


/// <summary>
/// Raised when a member's role is changed
/// </summary>
public class TeamMemberRoleChangedEvent : DomainEventBase
{
    public TeamMemberId MemberId { get; init; }
    public Guid UserId { get; init; }
    public TeamRole OldRole { get; init; }
    public TeamRole NewRole { get; init; }

    public TeamMemberRoleChangedEvent(
        TeamId teamId,
        TeamMemberId memberId,
        Guid userId,
        TeamRole oldRole,
        TeamRole newRole,
        DateTime occurredAt)
    {
        MemberId = memberId;
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}