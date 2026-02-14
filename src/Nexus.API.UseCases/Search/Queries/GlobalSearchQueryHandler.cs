using Ardalis.Result;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Common;
using Nexus.API.UseCases.Search.DTOs;

namespace Nexus.API.UseCases.Search.Queries;

/// <summary>
/// Handler for global search across Documents, Diagrams, and CodeSnippets.
/// Uses the ISearchService (Elasticsearch) for full-text search with facets.
///
/// Register as Scoped:
///   services.AddScoped&lt;GlobalSearchQueryHandler&gt;();
/// </summary>
public class GlobalSearchQueryHandler
{
    private readonly ISearchService _searchService;

    public GlobalSearchQueryHandler(ISearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    public async Task<Result<GlobalSearchResponse>> Handle(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result.Invalid(new ValidationError("Query is required"));

        if (request.PageSize < 1 || request.PageSize > 100)
            return Result.Invalid(new ValidationError("PageSize must be between 1 and 100"));

        // Perform search
        var searchResponse = await _searchService.SearchAsync(
            request.Query,
            request.Types,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var resultDtos = searchResponse.Results
            .Select(r => r.ToDto())
            .ToList();

        var response = new GlobalSearchResponse
        {
            Query = searchResponse.Query,
            Results = resultDtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalItems = searchResponse.TotalCount,
                TotalPages = (int)Math.Ceiling((double)searchResponse.TotalCount / request.PageSize),
                HasNextPage = request.Page < (int)Math.Ceiling((double)searchResponse.TotalCount / request.PageSize),
                HasPreviousPage = request.Page > 1
            },
            Facets = searchResponse.Facets.ToDto()
        };

        return Result.Success(response);
    }
}

/// <summary>
/// Response model for the GlobalSearch endpoint.
/// Combines paginated results with facets.
/// </summary>
public record GlobalSearchResponse
{
    public string Query { get; init; } = string.Empty;
    public List<SearchResultDto> Results { get; init; } = new();
    public PaginationMeta Pagination { get; init; } = new();
    public SearchFacetsDto Facets { get; init; } = new();
}
