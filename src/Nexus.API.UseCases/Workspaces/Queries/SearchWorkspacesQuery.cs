using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Queries;

/// <summary>
/// Query to search workspaces by name
/// </summary>
public record SearchWorkspacesQuery(
  string SearchTerm,
  Guid? TeamId = null) : IRequest<Result<List<WorkspaceDto>>>;
