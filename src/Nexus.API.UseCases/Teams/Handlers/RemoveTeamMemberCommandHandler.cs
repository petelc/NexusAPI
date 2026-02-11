using MediatR;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Teams.Commands;

namespace Nexus.API.UseCases.Teams.Handlers;

/// <summary>
/// Handler for removing a member from a team
/// </summary>
public sealed class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Result>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveTeamMemberCommandHandler> _logger;

    public RemoveTeamMemberCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<RemoveTeamMemberCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var team = await _teamRepository.GetByIdWithMembersAsync(TeamId.Create(request.TeamId), cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            var canManage = team.CanManageMembers(currentUserId);
            var isRemovingSelf = request.UserId == currentUserId;

            if (!canManage && !isRemovingSelf)
            {
                return Result.Unauthorized();
            }

            team.RemoveMember(request.UserId);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            _logger.LogInformation("User {UserId} removed from team {TeamId}", request.UserId, team.Id.Value);

            return Result.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from team {TeamId}", request.TeamId);
            return Result.Error(ex.Message);
        }
    }
}
