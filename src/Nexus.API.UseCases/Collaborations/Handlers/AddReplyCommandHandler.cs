using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for adding a reply to a comment.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// </summary>
public class AddReplyCommandHandler : IRequestHandler<AddReplyCommand, Result<CommentResponseDto>>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public AddReplyCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<CommentResponseDto>> Handle(
        AddReplyCommand command,
        CancellationToken cancellationToken)
    {
        var parentComment = await _collaborationRepository.GetCommentByIdAsync(
            command.ParentCommentId, cancellationToken);

        if (parentComment == null)
            return Result<CommentResponseDto>.NotFound("Parent comment not found");

        if (parentComment.IsDeleted)
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Cannot reply to a deleted comment" });

        if (string.IsNullOrWhiteSpace(command.Text))
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Reply text cannot be empty" });

        if (command.Text.Length > 2000)
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Reply text cannot exceed 2000 characters" });

        var reply = Comment.CreateReply(
            command.ParentCommentId,
            parentComment.ResourceType,
            parentComment.ResourceId,
            command.UserId,
            command.Text,
            parentComment.SessionId);

        await _collaborationRepository.AddCommentAsync(reply, cancellationToken);

        // Notify session + resource group of the new comment
        await _notificationService.NotifyCommentAddedAsync(
            new CommentNotificationDto
            {
                CommentId = reply.Id,
                ResourceId = reply.ResourceId,
                ResourceType = reply.ResourceType.ToString(),
                UserId = reply.UserId,
                Username = string.Empty,
                Text = reply.Text,
                Position = reply.Position,
                ParentCommentId = reply.ParentCommentId,
                CreatedAt = reply.CreatedAt,
                Action = "Added"
            },
            reply.SessionId,
            cancellationToken);

        // Targeted notification to the parent comment author (if different user)
        if (parentComment.UserId != command.UserId)
        {
            await _notificationService.NotifyReplyReceivedAsync(
                parentCommentAuthorUserId: parentComment.UserId,
                replyCommentId: reply.Id,
                parentCommentId: parentComment.Id,
                resourceId: reply.ResourceId,
                resourceType: reply.ResourceType.ToString(),
                replyAuthorUserId: command.UserId,
                replyText: reply.Text,
                createdAt: reply.CreatedAt,
                cancellationToken: cancellationToken);
        }

        return Result<CommentResponseDto>.Success(MapToResponseDto(reply));
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
            Username = string.Empty,
            FullName = string.Empty,
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
