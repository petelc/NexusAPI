using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Queries;

/// <summary>
/// Query to get all workspaces for a team
/// </summary>
public record GetTeamWorkspacesQuery(Guid TeamId) : IRequest<Result<List<WorkspaceDto>>>;
