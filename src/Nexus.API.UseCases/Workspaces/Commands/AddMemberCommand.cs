using Ardalis.Result;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to add a member to a workspace
/// </summary>
public record AddMemberCommand(
  Guid WorkspaceId,
  Guid UserId,
  string Role) : IRequest<Result<WorkspaceMemberDto>>;
