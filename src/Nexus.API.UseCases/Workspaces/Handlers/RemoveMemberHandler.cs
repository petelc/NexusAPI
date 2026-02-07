using Ardalis.Result;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.Commands;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for removing a member from a workspace
/// </summary>
public class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand, Result>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public RemoveMemberHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result> Handle(
    RemoveMemberCommand request,
    CancellationToken cancellationToken)
  {
    var currentUserId = _currentUserService.UserId;
    if (currentUserId == null || currentUserId == Guid.Empty)
      return Result.Unauthorized();

    // Get workspace
    var workspaceId = WorkspaceId.Create(request.WorkspaceId);
    var workspace = await _workspaceRepository.GetByIdWithMembersAsync(workspaceId, cancellationToken);

    if (workspace == null)
      return Result.NotFound("Workspace not found");

    // Remove member
    try
    {
      workspace.RemoveMember(
        UserId.Create(request.UserId),
        UserId.Create(currentUserId.Value));

      await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

      return Result.Success();
    }
    catch (Exception ex)
    {
      return Result.Error(ex.Message);
    }
  }
}
