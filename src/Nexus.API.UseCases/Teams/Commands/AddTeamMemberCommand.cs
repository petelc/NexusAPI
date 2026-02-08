using MediatR;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to add a member to a team
/// </summary>
public sealed record AddTeamMemberCommand(
    Guid TeamId,
    Guid UserId,
    string Role) : IRequest<AddTeamMemberResult>;

/// <summary>
/// Result of adding a team member
/// </summary>
public sealed record AddTeamMemberResult(
    Guid MemberId,
    Guid UserId,
    string Role,
    Guid InvitedBy,
    DateTime JoinedAt);
