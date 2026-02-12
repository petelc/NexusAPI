using MediatR;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Teams.Commands;

namespace Nexus.API.UseCases.Teams.Handlers;

/// <summary>
/// Handler for deleting a team
/// </summary>
public sealed class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, Result>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTeamCommandHandler> _logger;

    public DeleteTeamCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteTeamCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var team = await _teamRepository.GetByIdWithMembersAsync(TeamId.Create(request.TeamId), cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            var userRole = team.GetMemberRole(userId);
            if (userRole != TeamRole.Owner)
            {
                return Result.Unauthorized();
            }

            team.Delete(userId);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            _logger.LogInformation("Team {TeamId} deleted by user {UserId}", team.Id.Value, userId);

            return Result.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", request.TeamId);
            return Result.Error(ex.Message);
        }
    }
}
