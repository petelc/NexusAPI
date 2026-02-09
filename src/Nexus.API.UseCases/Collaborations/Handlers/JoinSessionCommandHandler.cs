using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for joining a collaboration session
/// </summary>
public class JoinSessionCommandHandler
{
    private readonly ICollaborationRepository _collaborationRepository;

    public JoinSessionCommandHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<CollaborationSessionResponseDto>> Handle(
        JoinSessionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate role
        if (!Enum.TryParse<ParticipantRole>(command.Role, true, out var role))
        {
            return Result<CollaborationSessionResponseDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid role: {command.Role}" });
        }

        // Get session
        var session = await _collaborationRepository.GetSessionByIdAsync(command.SessionId, cancellationToken);
        if (session == null || !session.IsActive)
        {
            return Result<CollaborationSessionResponseDto>.NotFound("Session not found or not active");
        }

        // Add participant
        session.AddParticipant(command.UserId, role);

        // Update
        await _collaborationRepository.UpdateSessionAsync(session, cancellationToken);

        // Map and return
        var response = MapToResponseDto(session);
        return Result<CollaborationSessionResponseDto>.Success(response);
    }

    private static CollaborationSessionResponseDto MapToResponseDto(
        Core.Aggregates.CollaborationAggregate.CollaborationSession session)
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
                Username = string.Empty, // TODO: Fetch from user service
                FullName = string.Empty, // TODO: Fetch from user service
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
