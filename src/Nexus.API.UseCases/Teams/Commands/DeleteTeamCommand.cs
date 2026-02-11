using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Teams.Commands;

/// <summary>
/// Command to delete a team (soft delete)
/// </summary>
public sealed record DeleteTeamCommand(Guid TeamId) : IRequest<Result>;
