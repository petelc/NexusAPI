using MediatR;
using Ardalis.Result;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get a collaboration session by ID
/// </summary>
public record GetSessionByIdQuery : IRequest<Result<CollaborationSessionResponseDto>>
{
    public SessionId SessionId { get; init; }
}
