namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to update a comment
/// </summary>
public record UpdateCommentRequest
{
    public string Text { get; init; } = string.Empty;
}