using Nexus.API.Core.Entities;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Service for searching content across the application.
/// Provides unified search over Documents, Diagrams, and CodeSnippets.
///
/// Register as Singleton:
///   services.AddSingleton&lt;ISearchService, ElasticsearchService&gt;();
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Performs a global search across all content types.
    /// </summary>
    /// <param name="query">Search query string</param>
    /// <param name="types">Optional comma-separated types to filter (document,diagram,snippet)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search response with results, pagination, and facets</returns>
    Task<SearchResponse> SearchAsync(
        string query,
        string? types = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a document for search.
    /// </summary>
    Task IndexDocumentAsync(
        Guid documentId,
        string title,
        string content,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a diagram for search.
    /// </summary>
    Task IndexDiagramAsync(
        Guid diagramId,
        string title,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a code snippet for search.
    /// </summary>
    Task IndexSnippetAsync(
        Guid snippetId,
        string title,
        string code,
        string language,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a resource from the search index.
    /// </summary>
    Task RemoveFromIndexAsync(
        string type,
        Guid resourceId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from a search query.
/// </summary>
public class SearchResponse
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResult> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public SearchFacets Facets { get; set; } = new();
}

/// <summary>
/// Faceted counts for filtering search results.
/// </summary>
public class SearchFacets
{
    public Dictionary<string, int> Types { get; set; } = new();
    public Dictionary<string, int> Tags { get; set; } = new();
}
