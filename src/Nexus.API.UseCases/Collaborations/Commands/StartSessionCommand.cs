using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to start a new collaboration session
/// </summary>
public record StartSessionCommand : IRequest<Result<CollaborationSessionResponseDto>>
{
    public string ResourceType { get; init; } = string.Empty;
    public ResourceId ResourceId { get; init; }
    public ParticipantId InitiatorUserId { get; init; }
}
