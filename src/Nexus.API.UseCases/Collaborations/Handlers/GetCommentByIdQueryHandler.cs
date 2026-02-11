using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Queries;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Aggregates.CollaborationAggregate;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for getting a single comment by ID with all replies
/// </summary>
public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, Result<CommentResponseDto>>
{
    private readonly ICollaborationRepository _collaborationRepository;

    public GetCommentByIdQueryHandler(ICollaborationRepository collaborationRepository)
    {
        _collaborationRepository = collaborationRepository ?? throw new ArgumentNullException(nameof(collaborationRepository));
    }

    public async Task<Result<CommentResponseDto>> Handle(
        GetCommentByIdQuery query,
        CancellationToken cancellationToken)
    {
        // Get the comment
        var comment = await _collaborationRepository.GetCommentByIdAsync(query.CommentId, cancellationToken);

        if (comment == null)
        {
            return Result<CommentResponseDto>.NotFound("Comment not found");
        }

        // Get all comments for the same resource to build reply hierarchy
        var allComments = await _collaborationRepository.GetResourceCommentsAsync(
            comment.ResourceType,
            ResourceId.Create(comment.ResourceId),
            false,
            cancellationToken);

        // Map to DTO with replies
        var response = MapToResponseDto(comment);
        response.Replies = GetReplies(comment.Id, allComments);

        return Result<CommentResponseDto>.Success(response);
    }

    private static List<CommentResponseDto> GetReplies(
        Guid parentId,
        IEnumerable<Comment> allComments)
    {
        var replies = allComments
            .Where(c => c.ParentCommentId == parentId)
            .Select(c =>
            {
                var dto = MapToResponseDto(c);
                dto.Replies = GetReplies(c.Id, allComments); // Recursive for nested replies
                return dto;
            })
            .ToList();

        return replies;
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
            Replies = new List<CommentResponseDto>()
        };
    }
}
