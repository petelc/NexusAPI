namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to update cursor position
/// </summary>
public record UpdateCursorPositionRequest
{
    public int? CursorPosition { get; init; }
}