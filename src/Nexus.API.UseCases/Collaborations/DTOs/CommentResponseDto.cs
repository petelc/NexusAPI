using Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response DTO for comment
/// </summary>
public record CommentResponseDto
{
    public Guid CommentId { get; init; }
    public Guid? SessionId { get; init; }
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int? Position { get; init; }
    public Guid? ParentCommentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<CommentResponseDto> Replies { get; init; } = new();
}