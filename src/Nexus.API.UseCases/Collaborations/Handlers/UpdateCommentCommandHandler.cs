using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for updating a comment.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// </summary>
public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Result<CommentResponseDto>>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public UpdateCommentCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<CommentResponseDto>> Handle(
        UpdateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var comment = await _collaborationRepository.GetCommentByIdAsync(
            command.CommentId, cancellationToken);

        if (comment == null)
            return Result<CommentResponseDto>.NotFound("Comment not found");

        if (comment.UserId != command.UserId)
            return Result<CommentResponseDto>.Forbidden();

        if (comment.IsDeleted)
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Cannot update a deleted comment" });

        if (string.IsNullOrWhiteSpace(command.Text))
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Comment text cannot be empty" });

        if (command.Text.Length > 2000)
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Comment text cannot exceed 2000 characters" });

        comment.UpdateText(command.Text);

        await _collaborationRepository.UpdateCommentAsync(comment, cancellationToken);

        var response = new CommentResponseDto
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
            Replies = new List<CommentResponseDto>()
        };

        await _notificationService.NotifyCommentUpdatedAsync(
            new CommentNotificationDto
            {
                CommentId = comment.Id,
                ResourceId = comment.ResourceId,
                ResourceType = comment.ResourceType.ToString(),
                UserId = comment.UserId,
                Username = string.Empty,
                Text = comment.Text,
                Position = comment.Position,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                Action = "Updated"
            },
            comment.SessionId,
            cancellationToken);

        return Result<CommentResponseDto>.Success(response);
    }
}
