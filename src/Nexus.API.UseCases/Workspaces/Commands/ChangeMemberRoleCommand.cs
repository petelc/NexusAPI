using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to change a member's role in a workspace
/// </summary>
public record ChangeMemberRoleCommand(
  Guid WorkspaceId,
  Guid UserId,
  string NewRole) : IRequest<Result<WorkspaceMemberDto>>;
