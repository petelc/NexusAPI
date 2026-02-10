using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Collaboration.Commands;

/// <summary>
/// Command to add a reply to an existing comment
/// </summary>
public record AddReplyCommand(
    CommentId ParentCommentId,
    UserId UserId,
    string Text);
