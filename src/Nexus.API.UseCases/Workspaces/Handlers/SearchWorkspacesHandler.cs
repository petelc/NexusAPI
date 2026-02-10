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
/// Handler for searching workspaces by name
/// </summary>
public class SearchWorkspacesHandler : IRequestHandler<SearchWorkspacesQuery, Result<List<WorkspaceDto>>>
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly ICurrentUserService _currentUserService;

  public SearchWorkspacesHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUserService currentUserService)
  {
    _workspaceRepository = workspaceRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<List<WorkspaceDto>>> Handle(
    SearchWorkspacesQuery request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;
    if (userId == null || userId == Guid.Empty)
      return Result.Unauthorized();

    // Search workspaces
    var teamId = request.TeamId.HasValue ? (TeamId?)TeamId.Create(request.TeamId.Value) : null;
    var workspaces = await _workspaceRepository.SearchByNameAsync(
      request.SearchTerm,
      teamId,
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
