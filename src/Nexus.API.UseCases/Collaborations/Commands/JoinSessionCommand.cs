using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to join an existing collaboration session
/// </summary>
public record JoinSessionCommand : IRequest<Result>
{
    public SessionId SessionId { get; init; }
    public ParticipantId UserId { get; init; }
    public string Role { get; init; } = "Viewer"; // "Viewer" or "Editor"
}
