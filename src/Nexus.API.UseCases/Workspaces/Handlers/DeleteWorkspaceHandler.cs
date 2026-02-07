using Ardalis.Result;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.Commands;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for deleting a workspace
/// </summary>
public class DeleteWorkspaceHandler : IRequestHandler<DeleteWorkspaceCommand, Result>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public DeleteWorkspaceHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result> Handle(
    DeleteWorkspaceCommand request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;
    if (userId == null || userId == Guid.Empty)
      return Result.Unauthorized();

    // Get workspace
    var workspaceId = WorkspaceId.Create(request.WorkspaceId);
    var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

    if (workspace == null)
      return Result.NotFound("Workspace not found");

    // Only owners can delete workspace
    var userIdObj = UserId.Create(userId.Value);
    var role = workspace.GetMemberRole(userIdObj);

    if (role != Core.Enums.WorkspaceMemberRole.Owner)
      return Result.Forbidden();

    // Soft delete
    workspace.Delete();
    await _workspaceRepository.DeleteAsync(workspace, cancellationToken);

    return Result.Success();
  }
}
