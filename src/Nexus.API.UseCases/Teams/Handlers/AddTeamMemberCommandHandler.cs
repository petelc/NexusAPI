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
/// Handler for adding a member to a team
/// </summary>
public sealed class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, Result<TeamMemberDto>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddTeamMemberCommandHandler> _logger;

    public AddTeamMemberCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<AddTeamMemberCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TeamMemberDto>> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
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

            if (!Enum.TryParse<TeamRole>(request.Role, ignoreCase: true, out var role))
            {
                return Result.Error($"Invalid role: {request.Role}. Valid roles are: Member, Admin, Owner");
            }

            team.AddMember(request.UserId, role, userId);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            var addedMember = team.GetMember(request.UserId);
            if (addedMember == null)
            {
                return Result.Error("Failed to retrieve added member");
            }

            _logger.LogInformation("User {UserId} added to team {TeamId} with role {Role}", request.UserId, team.Id.Value, role);

            var result = new TeamMemberDto
            {
                UserId = addedMember.UserId,
                Role = addedMember.Role.ToString(),
                JoinedAt = addedMember.JoinedAt,
                IsActive = addedMember.IsActive
            };

            return Result.Success(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to team {TeamId}", request.TeamId);
            return Result.Error(ex.Message);
        }
    }
}
