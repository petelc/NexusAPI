namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Request to start a collaboration session
/// </summary>
public record StartSessionRequest
{
    public string ResourceType { get; init; } = string.Empty; // "Document" or "Diagram"
    public Guid ResourceId { get; init; }
}