using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to update a workspace
/// </summary>
public record UpdateWorkspaceCommand(
  Guid WorkspaceId,
  string? Name,
  string? Description) : IRequest<Result<WorkspaceDto>>;
