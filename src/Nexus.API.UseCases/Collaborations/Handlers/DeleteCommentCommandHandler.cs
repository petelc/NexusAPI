using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Interfaces;

namespace Nexus.API.UseCases.Collaboration.Handlers;

/// <summary>
/// Handler for soft-deleting a comment.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// </summary>
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public DeleteCommentCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result> Handle(
        DeleteCommentCommand command,
        CancellationToken cancellationToken)
    {
        var comment = await _collaborationRepository.GetCommentByIdAsync(
            command.CommentId, cancellationToken);

        if (comment == null)
            return Result.NotFound("Comment not found");

        if (comment.UserId != command.UserId)
            return Result.Forbidden();

        if (comment.IsDeleted)
            return Result.Invalid(new ValidationError { ErrorMessage = "Comment is already deleted" });

        comment.Delete();

        await _collaborationRepository.UpdateCommentAsync(comment, cancellationToken);

        await _notificationService.NotifyCommentDeletedAsync(
            new CommentNotificationDto
            {
                CommentId = comment.Id,
                ResourceId = comment.ResourceId,
                ResourceType = comment.ResourceType.ToString(),
                UserId = comment.UserId,
                Username = string.Empty,
                Text = string.Empty,            // Do not send deleted content
                Position = comment.Position,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                Action = "Deleted"
            },
            comment.SessionId,
            cancellationToken);

        return Result.Success();
    }
}
