namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to join a session
/// </summary>
public record JoinSessionRequest
{
    public Guid SessionId { get; init; }
    public string Role { get; init; } = "Viewer"; // "Viewer" or "Editor"
}