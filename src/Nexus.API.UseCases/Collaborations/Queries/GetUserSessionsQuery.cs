using MediatR;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get sessions where user is a participant
/// </summary>
public record GetUserSessionsQuery : IRequest<ActiveSessionsResponseDto>
{
    public ParticipantId UserId { get; init; }
    public bool ActiveOnly { get; init; } = true;
}
