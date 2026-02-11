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
/// Handler for adding a comment to a resource.
/// Depends only on UseCases-layer interfaces - Clean Architecture compliant.
/// </summary>
public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<CommentResponseDto>>
{
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICollaborationNotificationService _notificationService;

    public AddCommentCommandHandler(
        ICollaborationRepository collaborationRepository,
        ICollaborationNotificationService notificationService)
    {
        _collaborationRepository = collaborationRepository
            ?? throw new ArgumentNullException(nameof(collaborationRepository));
        _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<CommentResponseDto>> Handle(
        AddCommentCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResourceType>(command.ResourceType, true, out var resourceType))
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type: {command.ResourceType}" });

        if (string.IsNullOrWhiteSpace(command.Text))
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Comment text cannot be empty" });

        if (command.Text.Length > 2000)
            return Result<CommentResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Comment text cannot exceed 2000 characters" });

        var comment = Comment.Create(
            SessionId.Create(command.SessionId.HasValue ? command.SessionId.Value : Guid.Empty),
            resourceType,
            command.ResourceId,
            command.UserId,
            command.Text,
            command.Position);

        await _collaborationRepository.AddCommentAsync(comment, cancellationToken);

        var response = MapToResponseDto(comment);

        await _notificationService.NotifyCommentAddedAsync(
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
                Action = "Added"
            },
            comment.SessionId,
            cancellationToken);

        return Result<CommentResponseDto>.Success(response);
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
