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
/// Handler for retrieving a team by ID
/// </summary>
public sealed class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, Result<TeamDto>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTeamByIdQueryHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var team = await _teamRepository.GetByIdWithMembersAsync(TeamId.Create(request.TeamId), cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            // Check if user is a member
            if (!team.IsMember(userId))
            {
                return Result.Unauthorized();
            }

            var dto = new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                CreatedBy = team.CreatedBy,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                Members = team.Members
                    .Where(m => m.IsActive)
                    .Select(m => new TeamMemberDto
                    {
                        MemberId = m.Id,
                        UserId = m.UserId,
                        Role = m.Role.ToString(),
                        RoleValue = (int)m.Role,
                        InvitedBy = m.InvitedBy,
                        JoinedAt = m.JoinedAt,
                        IsActive = m.IsActive
                    })
                    .OrderByDescending(m => m.RoleValue)
                    .ThenBy(m => m.JoinedAt)
                    .ToList(),
                MemberCount = team.Members.Count(m => m.IsActive),
                OwnerCount = team.Members.Count(m => m.IsActive && m.Role == TeamRole.Owner)
            };

            return Result.Success(dto);
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

