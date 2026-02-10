namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to reply to a comment
/// </summary>
public record ReplyToCommentRequest
{
    public string Text { get; init; } = string.Empty;
}