using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for starting a collaboration session.
/// Depends only on UseCases-layer interfaces — Clean Architecture compliant.
/// </summary>
public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Result<CollaborationSessionResponseDto>>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public StartSessionCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<CollaborationSessionResponseDto>> Handle(
        StartSessionCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResourceType>(command.ResourceType, true, out var resourceType))
        {
            return Result<CollaborationSessionResponseDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type: {command.ResourceType}" });
        }

        var existingSessions = await _collaborationRepository.GetActiveSessionsByResourceAsync(
            resourceType, command.ResourceId, cancellationToken);

        if (existingSessions.Any())
        {
            return Result<CollaborationSessionResponseDto>.Conflict(
                $"An active collaboration session already exists for this {command.ResourceType}");
        }

        var session = CollaborationSession.Start(
            resourceType,
            command.ResourceId,
            command.InitiatorUserId);

        await _collaborationRepository.AddSessionAsync(session, cancellationToken);

        // Real-time notification — interface-based, no Web dependency
        await _notificationService.NotifySessionStartedAsync(
            session.Id,
            session.ResourceId,
            session.ResourceType.ToString(),
            command.InitiatorUserId,
            session.StartedAt,
            cancellationToken);

        return Result<CollaborationSessionResponseDto>.Success(MapToResponseDto(session));
    }

    private static CollaborationSessionResponseDto MapToResponseDto(CollaborationSession session)
    {
        return new CollaborationSessionResponseDto
        {
            SessionId = session.Id,
            ResourceType = session.ResourceType.ToString(),
            ResourceId = session.ResourceId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            IsActive = session.IsActive,
            ActiveParticipantCount = session.GetActiveParticipantCount(),
            Participants = session.Participants.Select(p => new SessionParticipantDto
            {
                ParticipantId = p.Id,
                UserId = p.UserId,
                Username = string.Empty,
                FullName = string.Empty,
                Role = p.Role.ToString(),
                JoinedAt = p.JoinedAt,
                LeftAt = p.LeftAt,
                LastActivityAt = p.LastActivityAt,
                CursorPosition = p.CursorPosition,
                IsActive = p.IsActive
            }).ToList()
        };
    }
}
