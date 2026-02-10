using MediatR;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to leave a collaboration session
/// </summary>
public record LeaveSessionCommand : IRequest<bool>
{
    public SessionId SessionId { get; init; }
    public ParticipantId UserId { get; init; }
}
