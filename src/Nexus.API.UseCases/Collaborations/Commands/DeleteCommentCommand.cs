using Nexus.API.Core.ValueObjects;
using MediatR;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to delete a comment (soft delete)
/// </summary>
public record DeleteCommentCommand(
    CommentId CommentId,
    UserId UserId) : IRequest<Result>;
