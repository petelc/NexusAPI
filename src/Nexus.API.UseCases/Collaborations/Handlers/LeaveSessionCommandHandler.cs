using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for removing a participant from a collaboration session via REST.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// Note: WebSocket disconnect is handled separately by CollaborationHub.
/// </summary>
public class LeaveSessionCommandHandler : IRequestHandler<LeaveSessionCommand, Result>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public LeaveSessionCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result> Handle(
        LeaveSessionCommand command,
        CancellationToken cancellationToken)
    {
        var session = await _collaborationRepository.GetSessionByIdAsync(
            command.SessionId, cancellationToken);

        if (session == null)
            return Result.NotFound("Collaboration session not found");

        if (!session.IsActive)
            return Result.Invalid(new ValidationError { ErrorMessage = "Session is not active" });

        var participant = session.Participants.FirstOrDefault(p => p.UserId == command.UserId && p.IsActive);
        if (participant == null)
            return Result.NotFound("User is not an active participant in this session");

        session.RemoveParticipant(command.UserId);

        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        await _notificationService.NotifyParticipantRemovedAsync(
            session.Id,
            command.UserId,
            session.GetActiveParticipantCount(),
            cancellationToken);

        return Result.Success();
    }
}
