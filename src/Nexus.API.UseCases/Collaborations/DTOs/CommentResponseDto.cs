using Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response DTO for comment
/// </summary>
public class CommentResponseDto
{
    public Guid CommentId { get; set; }
    public Guid? SessionId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty; // TODO: Fetch from user service
    public string FullName { get; set; } = string.Empty; // TODO: Fetch from user service
    public string Text { get; set; } = string.Empty;
    public int? Position { get; set; }
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<CommentResponseDto> Replies { get; set; } = new();
}