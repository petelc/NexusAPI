using Ardalis.Result;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Workspaces.DTOs;
using Nexus.API.UseCases.Workspaces.Queries;

namespace Nexus.API.UseCases.Workspaces.Handlers;

/// <summary>
/// Handler for getting all workspaces for the current user
/// </summary>
public class GetUserWorkspacesHandler : IRequestHandler<GetUserWorkspacesQuery, Result<List<WorkspaceDto>>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public GetUserWorkspacesHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<List<WorkspaceDto>>> Handle(
    GetUserWorkspacesQuery request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;
    if (userId == null || userId == Guid.Empty)
      return Result.Unauthorized();

    // Get workspaces
    var workspaces = await _workspaceRepository.GetByUserIdAsync(
      UserId.Create(userId.Value),
      cancellationToken);

    // Map to DTOs
    var dtos = workspaces.Select(w => new WorkspaceDto
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
