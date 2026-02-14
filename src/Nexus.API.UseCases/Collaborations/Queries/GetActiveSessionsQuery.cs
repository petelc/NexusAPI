using MediatR;
using Ardalis.Result;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get active sessions for a resource
/// </summary>
public record GetActiveSessionsQuery : IRequest<Result<IEnumerable<CollaborationSessionResponseDto>>>
{
    public string ResourceType { get; init; } = string.Empty;
    public ResourceId ResourceId { get; init; }
}
