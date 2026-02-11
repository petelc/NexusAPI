using Nexus.API.Core.ValueObjects;
using MediatR;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to add a comment to a resource
/// </summary>
public record AddCommentCommand(
    string ResourceType,
    SessionId? SessionId,
    ResourceId ResourceId,
    UserId UserId,
    string Text,
    int? Position) : IRequest<Result<CommentResponseDto>>;
