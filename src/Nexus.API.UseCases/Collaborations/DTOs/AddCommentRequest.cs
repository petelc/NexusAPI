namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request DTO for adding a comment
/// </summary>
public class AddCommentRequest
{
    public Guid? SessionId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? Position { get; set; }
}
