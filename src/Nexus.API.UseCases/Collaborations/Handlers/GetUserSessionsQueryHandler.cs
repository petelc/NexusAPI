using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for getting user sessions
/// </summary>
public class GetUserSessionsQueryHandler
{
    private readonly ICollaborationRepository _collaborationRepository;

    public GetUserSessionsQueryHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<ActiveSessionsResponseDto>> Handle(
        ParticipantId userId,
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        var sessions = await _collaborationRepository.GetUserSessionsAsync(
            userId,
            activeOnly,
            cancellationToken);

        var response = new ActiveSessionsResponseDto
        {
            Sessions = sessions.Select(MapToResponseDto).ToList()
        };

        return Result<ActiveSessionsResponseDto>.Success(response);
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
