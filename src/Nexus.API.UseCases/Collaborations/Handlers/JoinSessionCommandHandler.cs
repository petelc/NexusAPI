using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for adding a participant to a collaboration session via REST.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// Note: WebSocket join is handled separately by CollaborationHub.JoinSession.
/// </summary>
public class JoinSessionCommandHandler : IRequestHandler<JoinSessionCommand, Result>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public JoinSessionCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result> Handle(
        JoinSessionCommand command,
        CancellationToken cancellationToken)
    {
        var session = await _collaborationRepository.GetSessionByIdAsync(
            command.SessionId, cancellationToken);

        if (session == null)
            return Result.NotFound("Collaboration session not found");

        if (!session.IsActive)
            return Result.Invalid(new ValidationError { ErrorMessage = "Cannot join an ended session" });

        if (!Enum.TryParse<Core.Enums.ParticipantRole>(command.Role, true, out var role))
            return Result.Invalid(new ValidationError { ErrorMessage = $"Invalid role: {command.Role}" });

        session.AddParticipant(command.UserId, role);

        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        await _notificationService.NotifyParticipantAddedAsync(
            session.Id,
            command.UserId,
            role.ToString(),
            session.GetActiveParticipantCount(),
            cancellationToken);

        return Result.Success();
    }
}
