using MediatR;
using Ardalis.Result;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get sessions where user is a participant
/// </summary>
public record GetUserSessionsQuery : IRequest<Result<IEnumerable<CollaborationSessionResponseDto>>>
{
    public ParticipantId UserId { get; init; }
    public bool ActiveOnly { get; init; } = true;
}
