using Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Response DTO for a list of comments
/// </summary>
public class CommentsResponseDto
{
    public List<CommentResponseDto> Comments { get; set; } = new();
    public int TotalCount { get; set; }
}