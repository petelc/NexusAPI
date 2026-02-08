using MediatR;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Queries;

/// <summary>
/// Query to search teams by name
/// </summary>
public sealed record SearchTeamsQuery(string SearchTerm) : IRequest<List<TeamSummaryDto>>;
