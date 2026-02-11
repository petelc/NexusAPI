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

    public async Task<Result<ChangeTeamMemberRoleResult>> Handle(
        Guid teamId,
        Guid targetUserId,
        string newRole,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

            var team = await _teamRepository.GetByIdWithMembersAsync(TeamId.Create(teamId), cancellationToken);

            if (team == null)
            {
                return Result.NotFound("Team not found");
            }

            if (!team.CanManageMembers(currentUserId))
            {
                return Result.Unauthorized();
            }

            if (!Enum.TryParse<TeamRole>(newRole, ignoreCase: true, out var role))
            {
                return Result.Error($"Invalid role: {newRole}. Valid roles are: Member, Admin, Owner");
            }

            var member = team.GetMember(targetUserId);
            if (member == null)
            {
                return Result.Error($"User {targetUserId} is not a member of this team");
            }

            var oldRole = member.Role;
            team.ChangeMemberRole(targetUserId, role);
            await _teamRepository.UpdateAsync(team, cancellationToken);

            _logger.LogInformation(
                "User {UserId} role changed from {OldRole} to {NewRole} in team {TeamId}",
                targetUserId,
                oldRole,
                role,
                team.Id.Value);

            var result = new ChangeTeamMemberRoleResult(
                UserId: targetUserId,
                OldRole: oldRole.ToString(),
                NewRole: role.ToString());

            return Result.Success(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing member role in team {TeamId}", teamId);
            return Result.Error(ex.Message);
        }
    }
}
