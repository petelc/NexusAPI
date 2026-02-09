using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for starting a collaboration session
/// </summary>
public class StartSessionCommandHandler
{
    private readonly ICollaborationRepository _collaborationRepository;

    public StartSessionCommandHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<CollaborationSessionResponseDto>> Handle(
        StartSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Validate resource type
        if (!Enum.TryParse<ResourceType>(request.ResourceType, true, out var resourceType))
        {
            return Result<CollaborationSessionResponseDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type: {request.ResourceType}" });
        }

        // Check if there's already an active session for this resource
        var existingSessions = await _collaborationRepository.GetActiveSessionsByResourceAsync(
            resourceType,
            request.ResourceId,
            cancellationToken);

        if (existingSessions.Any())
        {
            return Result<CollaborationSessionResponseDto>.Conflict(
                $"An active collaboration session already exists for this {request.ResourceType}");
        }

        // Create new session
        var session = CollaborationSession.Start(
            resourceType,
            request.ResourceId,
            ParticipantId.Create(request.InitiatorUserId));

        // Save to database
        await _collaborationRepository.AddSessionAsync(session, cancellationToken);

        // Map to response DTO
        var response = MapToResponseDto(session);
        return Result<CollaborationSessionResponseDto>.Success(response);
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
