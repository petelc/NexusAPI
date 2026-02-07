using Ardalis.Result;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to remove a member from a workspace
/// </summary>
public record RemoveMemberCommand(
  Guid WorkspaceId,
  Guid UserId) : IRequest<Result>;
