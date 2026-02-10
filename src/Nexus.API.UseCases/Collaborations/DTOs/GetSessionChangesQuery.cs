namespace Nexus.API.UseCases.Collaboration.DTOs;

/// <summary>
/// Query parameters for getting session changes
/// </summary>
public record GetSessionChangesQuery
{
    public Guid SessionId { get; init; }
    public DateTime? Since { get; init; }
}