namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to apply a change
/// </summary>
public record ApplyChangeRequest
{
    public string ChangeType { get; init; } = string.Empty; // "Insert", "Update", "Delete"
    public int Position { get; init; }
    public string? Data { get; init; }
}