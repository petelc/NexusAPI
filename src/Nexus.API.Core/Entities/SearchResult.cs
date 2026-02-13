namespace Nexus.API.Core.Entities;

/// <summary>
/// Read-only entity representing a search result.
/// Created by ISearchService.SearchAsync.
/// </summary>
public class SearchResult
{
    public string Type { get; init; } = string.Empty; // "document" | "diagram" | "snippet"
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public double Score { get; init; }
    public List<string> Highlights { get; init; } = new();
    public string CreatedByUsername { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string SelfLink => $"/api/v1/{Type}s/{Id}";
}
