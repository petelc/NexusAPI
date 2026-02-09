using MediatR;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get active sessions for a resource
/// </summary>
public record GetActiveSessionsQuery : IRequest<ActiveSessionsResponseDto>
{
    public string ResourceType { get; init; } = string.Empty;
    public ResourceId ResourceId { get; init; }
}
