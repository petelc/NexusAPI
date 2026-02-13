namespace Nexus.API.UseCases.Search.DTOs;

/// <summary>
/// Request for the global search endpoint.
/// </summary>
public record SearchRequest
{
    public string Query { get; init; } = string.Empty;
    public string? Types { get; init; } // Comma-separated: "document,diagram,snippet"
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Individual search result DTO for the API response.
/// </summary>
public record SearchResultDto
{
    public string Type { get; init; } = string.Empty;
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public double Score { get; init; }
    public List<string> Highlights { get; init; } = new();
    public CreatorDto CreatedBy { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public LinkDto Links { get; init; } = new();
}

public record CreatorDto
{
    public string Username { get; init; } = string.Empty;
}

public record LinkDto
{
    public string Self { get; init; } = string.Empty;
}

/// <summary>
/// Facets returned with search results for filtering UI.
/// </summary>
public record SearchFacetsDto
{
    public Dictionary<string, int> Types { get; init; } = new();
    public Dictionary<string, int> Tags { get; init; } = new();
}

/// <summary>
/// Mapping extensions for SearchResult → SearchResultDto.
/// </summary>
public static class SearchMappingExtensions
{
    public static SearchResultDto ToDto(this Core.Entities.SearchResult result)
    {
        return new SearchResultDto
        {
            Type = result.Type,
            Id = result.Id,
            Title = result.Title,
            Excerpt = result.Excerpt,
            Score = result.Score,
            Highlights = result.Highlights,
            CreatedBy = new CreatorDto { Username = result.CreatedByUsername },
            CreatedAt = result.CreatedAt,
            Links = new LinkDto { Self = result.SelfLink }
        };
    }

    public static SearchFacetsDto ToDto(this Core.Interfaces.SearchFacets facets)
    {
        return new SearchFacetsDto
        {
            Types = facets.Types,
            Tags = facets.Tags
        };
    }
}
