using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to add a member to a team
/// </summary>
public sealed record AddTeamMemberCommand(
    Guid TeamId,
    Guid UserId,
    string Role) : IRequest<Result<TeamMemberDto>>;

/// <summary>
/// Result of adding a team member
/// </summary>
public sealed record AddTeamMemberResult(
    Guid MemberId,
    Guid UserId,
    string Role,
    Guid InvitedBy,
    DateTime JoinedAt);
