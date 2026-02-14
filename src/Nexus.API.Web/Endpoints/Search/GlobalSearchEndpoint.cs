using FastEndpoints;
using Nexus.API.UseCases.Search.DTOs;
using Nexus.API.UseCases.Search.Queries;
using GlobalSearchResponseDto = Nexus.API.UseCases.Search.DTOs.GlobalSearchResponse;

namespace Nexus.API.Web.Endpoints.Search;

/// <summary>
/// Global search endpoint across Documents, Diagrams, and CodeSnippets.
/// GET /api/v1/search
///
/// Query Parameters:
/// - query (required): Search term
/// - types (optional): Comma-separated list (document,diagram,snippet)
/// - page (optional, default 1): Page number
/// - pageSize (optional, default 20, max 100): Items per page
///
/// Returns 200 OK with search results, pagination, and facets.
/// </summary>
public class GlobalSearchEndpoint : Endpoint<SearchRequest, GlobalSearchResponseDto>
{
    private readonly GlobalSearchQueryHandler _handler;

    public GlobalSearchEndpoint(GlobalSearchQueryHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public override void Configure()
    {
        Get("/search");
        AllowAnonymous(); // Change to RequireAuth if needed
        Options(x => x
            .WithTags("Search")
            .WithSummary("Global search")
            .WithDescription("Search across documents, diagrams, and code snippets"));
    }

    public override async Task HandleAsync(SearchRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            return;
        }

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Validation failed",
                details = result.ValidationErrors.Select(e => new
                {
                    field = e.Identifier,
                    message = e.ErrorMessage
                }).ToList()
            }, ct);
            return;
        }

        HttpContext.Response.StatusCode = 500;
        await HttpContext.Response.WriteAsJsonAsync(new
        {
            error = result.Errors.FirstOrDefault() ?? "An error occurred"
        }, ct);
    }
}
