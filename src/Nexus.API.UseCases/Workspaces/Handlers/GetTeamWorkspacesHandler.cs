using Ardalis.Result;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.DTOs;
using Nexus.API.UseCases.Workspaces.Queries;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for getting all workspaces for a team
/// </summary>
public class GetTeamWorkspacesHandler : IRequestHandler<GetTeamWorkspacesQuery, Result<List<WorkspaceDto>>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public GetTeamWorkspacesHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<List<WorkspaceDto>>> Handle(
    GetTeamWorkspacesQuery request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;
    if (userId == null || userId == Guid.Empty)
      return Result.Unauthorized();

    // Get workspaces
    var workspaces = await _workspaceRepository.GetByTeamIdAsync(
      TeamId.Create(request.TeamId),
      cancellationToken);

    // Filter to only workspaces where user is a member
    var userIdObj = UserId.Create(userId.Value);
    var userWorkspaces = workspaces.Where(w => w.IsMember(userIdObj)).ToList();

    // Map to DTOs
    var dtos = userWorkspaces.Select(w => new WorkspaceDto
    {
      WorkspaceId = w.Id.Value,
      Name = w.Name,
      Description = w.Description,
      TeamId = w.TeamId.Value,
      CreatedBy = w.CreatedBy.Value,
      CreatedAt = w.CreatedAt,
      UpdatedAt = w.UpdatedAt,
      MemberCount = w.Members.Count(m => m.IsActive),
      Members = new List<WorkspaceMemberDto>() // Don't include members in list view
    }).ToList();

    return Result.Success(dtos);
  }
}
