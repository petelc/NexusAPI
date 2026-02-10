namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request DTO for updating a comment
/// </summary>
public class UpdateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}