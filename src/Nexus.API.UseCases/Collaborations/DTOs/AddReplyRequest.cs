namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request DTO for adding a reply
/// </summary>
public class AddReplyRequest
{
    public string Text { get; set; } = string.Empty;
}