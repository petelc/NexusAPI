using Nexus.API.Core.ValueObjects;
using MediatR;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to update an existing comment
/// </summary>
public record UpdateCommentCommand(
    CommentId CommentId,
    UserId UserId,
    string Text) : IRequest<Result<CommentResponseDto>>;
