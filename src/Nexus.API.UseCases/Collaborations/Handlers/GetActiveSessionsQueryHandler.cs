using MediatR;
using Ardalis.Result;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for getting active sessions for a resource
/// </summary>
public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, Result<IEnumerable<CollaborationSessionResponseDto>>>
{
    private readonly ICollaborationRepository _collaborationRepository;

    public GetActiveSessionsQueryHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<IEnumerable<CollaborationSessionResponseDto>>> Handle(
        GetActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResourceType>(request.ResourceType, true, out var parsedResourceType))
        {
            return Result<IEnumerable<CollaborationSessionResponseDto>>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type: {request.ResourceType}" });
        }

        var sessions = await _collaborationRepository.GetActiveSessionsByResourceAsync(
            parsedResourceType,
            request.ResourceId,
            cancellationToken);

        var response = sessions.Select(MapToResponseDto);

        return Result<IEnumerable<CollaborationSessionResponseDto>>.Success(response);
    }

    private static CollaborationSessionResponseDto MapToResponseDto(
        CollaborationSession session)
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
