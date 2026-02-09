using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for leaving a collaboration session
/// </summary>
public class LeaveSessionCommandHandler
{
    private readonly ICollaborationRepository _collaborationRepository;

    public LeaveSessionCommandHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result> Handle(
        SessionId sessionId,
        ParticipantId userId,
        CancellationToken cancellationToken)
    {
        var session = await _collaborationRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return Result.NotFound("Session not found");
        }

        if (!session.IsUserActiveParticipant(userId))
        {
            return Result.NotFound("You are not a participant in this session");
        }

        session.RemoveParticipant(userId);
        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        return Result.Success();
    }
}

