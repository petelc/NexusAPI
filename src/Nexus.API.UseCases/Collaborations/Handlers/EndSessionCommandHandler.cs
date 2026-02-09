using Ardalis.Result;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for ending a collaboration session
/// </summary>
public class EndSessionCommandHandler
{
    private readonly ICollaborationRepository _collaborationRepository;

    public EndSessionCommandHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result> Handle(
        SessionId sessionId,
        ParticipantId userId,
        CancellationToken cancellationToken)
    {
        // Get the session
        var session = await _collaborationRepository.GetSessionByIdAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return Result.NotFound("Session not found");
        }

        // Check if user is an active participant
        if (!session.IsUserActiveParticipant(userId))
        {
            return Result.Unauthorized();
        }

        // End the session
        session.End();

        // Update in database
        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        return Result.Success();
    }
}
