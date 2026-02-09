namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to add a comment
/// </summary>
public record AddCommentRequest
{
    public string ResourceType { get; init; } = string.Empty; // "Document" or "Diagram"
    public Guid ResourceId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int? Position { get; init; }
    public Guid? SessionId { get; init; }
}
