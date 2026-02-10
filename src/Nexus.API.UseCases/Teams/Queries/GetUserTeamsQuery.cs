using MediatR;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Queries;

/// <summary>
/// Query to get all teams the current user is a member of
/// </summary>
public sealed record GetUserTeamsQuery : IRequest<List<TeamSummaryDto>>;
