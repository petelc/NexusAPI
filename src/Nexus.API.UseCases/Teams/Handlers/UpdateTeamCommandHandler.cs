using MediatR;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Teams.Commands;
using Nexus.API.UseCases.Teams.DTOs;

namespace Nexus.API.UseCases.Teams.Handlers;

/// <summary>
/// Handler for updating team details
/// </summary>
public sealed class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, Result<TeamDto>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTeamCommandHandler> _logger;

    public UpdateTeamCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateTeamCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var teamId = TeamId.Create(request.TeamId);
            var team = await _teamRepository.GetByIdWithMembersAsync(teamId, cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            if (!team.CanManageMembers(userId))
            {
                return Result.Unauthorized();
            }

            // Check if new name conflicts
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != team.Name)
            {
                var nameExists = await _teamRepository.ExistsByNameAsync(request.Name, teamId, cancellationToken);
                if (nameExists)
                {
                    return Result.Error($"A team with the name '{request.Name}' already exists");
                }
            }

            team.Update(request.Name, request.Description);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            _logger.LogInformation("Team {TeamId} updated by user {UserId}", team.Id.Value, userId);

            var result = new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                CreatedBy = team.CreatedBy,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };
            return Result.Success(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", request.TeamId);
            return Result.Error(ex.Message);
        }
    }
}
