using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Queries;

/// <summary>
/// Query to get a team by ID with full details
/// </summary>
public sealed record GetTeamByIdQuery(Guid TeamId) : IRequest<Result<TeamDto>>;
