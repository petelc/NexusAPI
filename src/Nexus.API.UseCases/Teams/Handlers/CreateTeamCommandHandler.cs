using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Interfaces;
using Traxs.SharedKernel;
using Nexus.API.UseCases.Teams.Commands;

namespace Nexus.API.UseCases.Teams.Handlers;

/// <summary>
/// Handler for creating a new team
/// </summary>
public sealed class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<CreateTeamResult>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTeamCommandHandler> _logger;

    public CreateTeamCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateTeamCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<CreateTeamResult>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            // Check if team name already exists
            var nameExists = await _teamRepository.ExistsByNameAsync(request.Name, excludeId: null, cancellationToken);
            if (nameExists)
            {
                return Result.Error($"A team with the name '{request.Name}' already exists");
            }

            // Create the team
            var team = Team.Create(
                name: request.Name,
                description: request.Description,
                createdBy: userId);

            await _teamRepository.AddAsync(team, cancellationToken);

            _logger.LogInformation(
                "Team {TeamId} '{TeamName}' created by user {UserId}",
                team.Id.Value,
                team.Name,
                userId);

            var result = new CreateTeamResult(
                TeamId: team.Id,
                Name: team.Name,
                Description: team.Description,
                CreatedBy: team.CreatedBy,
                CreatedAt: team.CreatedAt);

            return Result.Success(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return Result.Error(ex.Message);
        }
    }
}


