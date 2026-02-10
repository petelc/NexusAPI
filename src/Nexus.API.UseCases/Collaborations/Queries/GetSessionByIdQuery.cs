using MediatR;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get a collaboration session by ID
/// </summary>
public record GetSessionByIdQuery : IRequest<CollaborationSessionResponseDto?>
{
    public SessionId SessionId { get; init; }
}
