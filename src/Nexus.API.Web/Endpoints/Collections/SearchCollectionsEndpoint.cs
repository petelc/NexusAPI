using MediatR;
using FastEndpoints;
using Nexus.API.UseCases.Collections.Queries;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/workspaces/{workspaceId}/collections/search
/// Searches collections by name or description
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class SearchCollectionsEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public SearchCollectionsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/workspaces/{workspaceId}/collections/search");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Search collections")
      .WithDescription("Searches collections by name or description"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("workspaceId"), out var workspaceId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid workspace ID" }, ct);
      return;
    }

    var searchTerm = Query<string>("searchTerm") ?? string.Empty;

    var query = new SearchCollectionsQuery
    {
      WorkspaceId = workspaceId,
      SearchTerm = searchTerm
    };

    try
    {
      var result = await _mediator.Send(query, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to search collections" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
