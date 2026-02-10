using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Queries;

/// <summary>
/// Query to get all workspaces for a user
/// </summary>
public record GetUserWorkspacesQuery() : IRequest<Result<List<WorkspaceDto>>>;
