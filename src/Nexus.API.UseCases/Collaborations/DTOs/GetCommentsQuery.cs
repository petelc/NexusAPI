namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Query parameters for getting comments
/// </summary>
public record GetCommentsQuery
{
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public bool IncludeDeleted { get; init; } = false;
}