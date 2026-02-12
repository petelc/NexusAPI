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
/// Handler for changing a team member's role
/// </summary>
public sealed class ChangeTeamMemberRoleCommandHandler : IRequestHandler<ChangeTeamMemberRoleCommand, Result<TeamMemberDto>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChangeTeamMemberRoleCommandHandler> _logger;

    public ChangeTeamMemberRoleCommandHandler(
        ITeamRepository teamRepository,
        ICurrentUserService currentUserService,
        ILogger<ChangeTeamMemberRoleCommandHandler> logger)
    {
        _teamRepository = teamRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TeamMemberDto>> Handle(
        ChangeTeamMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var team = await _teamRepository.GetByIdWithMembersAsync(TeamId.Create(request.TeamId), cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            if (!team.CanManageMembers(currentUserId))
            {
                return Result.Unauthorized();
            }

            if (!Enum.TryParse<TeamRole>(request.NewRole, ignoreCase: true, out var role))
            {
                return Result.Error($"Invalid role: {request.NewRole}. Valid roles are: Member, Admin, Owner");
            }

            var member = team.GetMember(request.UserId);
            if (member == null)
            {
                return Result.Error($"User {request.UserId} is not a member of this team");
            }

            var oldRole = member.Role;
            team.ChangeMemberRole(request.UserId, role);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            _logger.LogInformation(
                "User {UserId} role changed from {OldRole} to {NewRole} in team {TeamId}",
                request.UserId,
                oldRole,
                role,
                team.Id.Value);

            var result = new TeamMemberDto
            {
                UserId = request.UserId,
                Role = role.ToString(),
                JoinedAt = member.JoinedAt,
                IsActive = member.IsActive
            };

            return Result.Success(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing member role in team {TeamId}", request.TeamId);
            return Result.Error(ex.Message);
        }
    }
}
