namespace Nexus.API.UseCases.Search.DTOs;

/// <summary>
/// Request for global search endpoint.
/// </summary>
public class GlobalSearchRequest
{
    /// <summary>
    /// Search query string (required).
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Optional comma-separated content types to filter.
    /// Valid values: document, diagram, snippet
    /// Example: "document,snippet"
    /// </summary>
    public string? Types { get; set; }

    /// <summary>
    /// Page number (1-based, default: 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Items per page (default: 20, max: 100).
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response for global search endpoint.
/// </summary>
public class GlobalSearchResponse
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResultDto> Results { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
    public SearchFacetsDto Facets { get; set; } = new();
}

/// <summary>
/// Pagination metadata.
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
