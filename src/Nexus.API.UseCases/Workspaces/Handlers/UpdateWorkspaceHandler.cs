using Ardalis.Result;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for updating a workspace
/// </summary>
public class UpdateWorkspaceHandler : IRequestHandler<UpdateWorkspaceCommand, Result<WorkspaceDto>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public UpdateWorkspaceHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<WorkspaceDto>> Handle(
    UpdateWorkspaceCommand request,
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

    // Check if user can manage workspace (must be Admin or Owner)
    if (!workspace.CanManageMembers(UserId.Create(userId.Value)))
      return Result.Forbidden();

    // Update workspace
    try
    {
      workspace.Update(request.Name, request.Description);
      await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

      var dto = MapToDto(workspace);
      return Result.Success(dto);
    }
    catch (Exception ex)
    {
      return Result.Error(ex.Message);
    }
  }

  private static WorkspaceDto MapToDto(Workspace workspace)
  {
    return new WorkspaceDto
    {
      WorkspaceId = workspace.Id.Value,
      Name = workspace.Name,
      Description = workspace.Description,
      TeamId = workspace.TeamId.Value,
      CreatedBy = workspace.CreatedBy.Value,
      CreatedAt = workspace.CreatedAt,
      UpdatedAt = workspace.UpdatedAt,
      MemberCount = workspace.Members.Count(m => m.IsActive),
      Members = workspace.Members
        .Where(m => m.IsActive)
        .Select(m => new WorkspaceMemberDto
        {
          MemberId = m.Id.Value,
          UserId = m.UserId.Value,
          Role = m.Role.ToString(),
          JoinedAt = m.JoinedAt,
          InvitedBy = m.InvitedBy?.Value,
          IsActive = m.IsActive
        }).ToList()
    };
  }
}
