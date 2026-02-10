using MediatR;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to end a collaboration session
/// </summary>
public record EndSessionCommand : IRequest<bool>
{
    public SessionId SessionId { get; init; }
    public ParticipantId UserId { get; init; }
}
