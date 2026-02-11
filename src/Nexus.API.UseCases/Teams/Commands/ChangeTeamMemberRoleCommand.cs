using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to change a team member's role
/// </summary>
public sealed record ChangeTeamMemberRoleCommand(
    Guid TeamId,
    Guid UserId,
    string NewRole) : IRequest<Result<TeamMemberDto>>;

/// <summary>
/// Result of changing a member's role
/// </summary>
public sealed record ChangeTeamMemberRoleResult(
    Guid UserId,
    string OldRole,
    string NewRole);
