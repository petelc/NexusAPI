using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for ending a collaboration session.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// </summary>
public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, Result>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public EndSessionCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result> Handle(
        EndSessionCommand command,
        CancellationToken cancellationToken)
    {
        var session = await _collaborationRepository.GetSessionByIdAsync(
            command.SessionId, cancellationToken);

        if (session == null)
            return Result.NotFound("Collaboration session not found");

        var initiator = session.Participants.FirstOrDefault(p => p.IsActive);
        if (initiator == null || initiator.UserId != command.UserId)
            return Result.Unauthorized();

        session.End();

        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        await _notificationService.NotifySessionEndedAsync(
            session.Id,
            session.ResourceId,
            session.ResourceType.ToString(),
            command.UserId,
            session.EndedAt!.Value,
            cancellationToken);

        return Result.Success();
    }
}
