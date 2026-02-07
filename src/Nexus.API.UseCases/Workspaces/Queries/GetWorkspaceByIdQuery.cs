using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Queries;

/// <summary>
/// Query to get a workspace by ID
/// </summary>
public record GetWorkspaceByIdQuery(
  Guid WorkspaceId,
  bool IncludeMembers = false) : IRequest<Result<WorkspaceDto>>;
