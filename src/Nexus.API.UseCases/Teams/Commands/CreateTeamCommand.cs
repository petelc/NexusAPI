using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to create a new team
/// </summary>
public sealed record CreateTeamCommand(
    string Name,
    string? Description) : IRequest<Result<CreateTeamResult>>;

/// <summary>
/// Result of creating a team
/// </summary>
public sealed record CreateTeamResult(
    Guid TeamId,
    string Name,
    string? Description,
    Guid CreatedBy,
    DateTime CreatedAt);
