using MediatR;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Teams.DTOs;
using Nexus.API.UseCases.Teams.Queries;

namespace Nexus.API.UseCases.Teams.Handlers;

/// <summary>
/// Handler for retrieving user's teams
/// </summary>
public sealed class GetUserTeamsQueryHandler : IRequestHandler<GetUserTeamsQuery, Result<IEnumerable<TeamSummaryDto>>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserTeamsQueryHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<TeamSummaryDto>>> Handle(GetUserTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var teams = await _teamRepository.GetByUserIdAsync(userId, cancellationToken);

            var dtos = teams
                .Select(team => new TeamSummaryDto
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description,
                    CreatedBy = team.CreatedBy,
                    CreatedAt = team.CreatedAt,
                    UpdatedAt = team.UpdatedAt,
                    MemberCount = team.Members.Count(m => m.IsActive),
                    UserRole = team.GetMemberRole(userId)?.ToString()
                })
                .OrderBy(t => t.Name)
                .AsEnumerable();

            return Result.Success(dtos);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
