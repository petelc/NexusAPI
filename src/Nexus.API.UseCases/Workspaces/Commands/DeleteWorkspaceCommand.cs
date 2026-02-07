using Ardalis.Result;

namespace Nexus.API.UseCases.Workspaces.Commands;

/// <summary>
/// Command to delete a workspace
/// </summary>
public record DeleteWorkspaceCommand(Guid WorkspaceId) : IRequest<Result>;
