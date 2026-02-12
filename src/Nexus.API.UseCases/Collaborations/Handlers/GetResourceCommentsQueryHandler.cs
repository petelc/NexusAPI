using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for getting comments for a specific resource
/// </summary>
public class GetResourceCommentsQueryHandler : IRequestHandler<GetResourceCommentsQuery, Result<IEnumerable<CommentResponseDto>>>
{
    private readonly ICollaborationRepository _collaborationRepository;

    public GetResourceCommentsQueryHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<IEnumerable<CommentResponseDto>>> Handle(
        GetResourceCommentsQuery query,
        CancellationToken cancellationToken)
    {
        // Validate resource type
        if (!Enum.TryParse<ResourceType>(query.ResourceType, true, out var resourceType))
        {
            return Result<IEnumerable<CommentResponseDto>>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type: {query.ResourceType}" });
        }

        // Get comments from repository
        var comments = await _collaborationRepository.GetResourceCommentsAsync(
            resourceType,
            ResourceId.Create(query.ResourceId),
            query.IncludeDeleted,
            cancellationToken);

        // Filter to only top-level comments (replies will be nested)
        var topLevelComments = comments.Where(c => c.ParentCommentId == null).ToList();

        // Map to response DTO
        var response = topLevelComments.Select(MapToResponseDto);

        return Result<IEnumerable<CommentResponseDto>>.Success(response);
    }

    private static CommentResponseDto MapToResponseDto(Comment comment)
    {
        return new CommentResponseDto
        {
            CommentId = comment.Id,
            SessionId = comment.SessionId,
            ResourceType = comment.ResourceType.ToString(),
            ResourceId = comment.ResourceId,
            UserId = comment.UserId,
            Username = string.Empty, // TODO: Fetch from user service
            FullName = string.Empty, // TODO: Fetch from user service
            Text = comment.Text,
            Position = comment.Position,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsDeleted = comment.IsDeleted,
            DeletedAt = comment.DeletedAt,
            Replies = comment.Replies.Select(MapToResponseDto).ToList()
        };
    }
}
