using MediatR;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to remove a member from a team
/// </summary>
public sealed record RemoveTeamMemberCommand(
    Guid TeamId,
    Guid UserId) : IRequest;
