using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to create a new workspace
/// </summary>
public record CreateWorkspaceCommand(
  string Name,
  string? Description,
  Guid TeamId) : IRequest<Result<WorkspaceDto>>;
