using Ardalis.Result;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.DTOs;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for creating a new workspace
/// </summary>
public class CreateWorkspaceHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkspaceDto>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public CreateWorkspaceHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<WorkspaceDto>> Handle(
    CreateWorkspaceCommand request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;
    if (userId == null || userId == Guid.Empty)
      return Result.Unauthorized();

    // Check if workspace name already exists for this team
    var teamId = TeamId.Create(request.TeamId);
    var exists = await _workspaceRepository.ExistsByNameAndTeamAsync(
      request.Name,
      teamId,
      cancellationToken);

    if (exists)
      return Result.Error("A workspace with this name already exists for this team");

    // Create workspace
    var workspace = Workspace.Create(
      request.Name,
      request.Description,
      teamId,
      UserId.Create(userId.Value));

    // Save
    await _workspaceRepository.AddAsync(workspace, cancellationToken);

    // Map to DTO
    var dto = MapToDto(workspace);

    return Result.Success(dto);
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
