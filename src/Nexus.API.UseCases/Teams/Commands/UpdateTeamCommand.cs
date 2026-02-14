using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to update team details
/// </summary>
public sealed record UpdateTeamCommand(
    Guid TeamId,
    string? Name,
    string? Description) : IRequest<Result<TeamDto>>;

/// <summary>
/// Result of updating a team
/// </summary>
public sealed record UpdateTeamResult(
    Guid TeamId,
    string Name,
    string? Description,
    DateTime UpdatedAt);
