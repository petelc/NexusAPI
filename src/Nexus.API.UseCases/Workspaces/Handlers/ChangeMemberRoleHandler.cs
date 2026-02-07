using Ardalis.Result;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for changing a member's role in a workspace
/// </summary>
public class ChangeMemberRoleHandler : IRequestHandler<ChangeMemberRoleCommand, Result<WorkspaceMemberDto>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public ChangeMemberRoleHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<WorkspaceMemberDto>> Handle(
    ChangeMemberRoleCommand request,
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

    // Parse new role
    if (!Enum.TryParse<WorkspaceMemberRole>(request.NewRole, true, out var newRole))
      return Result.Error($"Invalid role: {request.NewRole}");

    // Change member role
    try
    {
      workspace.ChangeMemberRole(
        UserId.Create(request.UserId),
        newRole,
        UserId.Create(currentUserId.Value));

      await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

      // Find the updated member
      var member = workspace.Members.First(m => m.UserId == UserId.Create(request.UserId) && m.IsActive);

      var dto = new WorkspaceMemberDto
      {
        MemberId = member.Id.Value,
        UserId = member.UserId.Value,
        Role = member.Role.ToString(),
        JoinedAt = member.JoinedAt,
        InvitedBy = member.InvitedBy?.Value,
        IsActive = member.IsActive
      };

      return Result.Success(dto);
    }
    catch (Exception ex)
    {
      return Result.Error(ex.Message);
    }
  }
}
