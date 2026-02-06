using FastEndpoints;
using Nexus.API.UseCases.Collections.Queries;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/collections/{id}/breadcrumb
/// Gets collection breadcrumb (ancestor chain)
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetCollectionBreadcrumbEndpoint : EndpointWithoutRequest
{
  private readonly GetCollectionBreadcrumbHandler _handler;

  public GetCollectionBreadcrumbEndpoint(GetCollectionBreadcrumbHandler handler)
  {
    _handler = handler;
  }

  public override void Configure()
  {
    Get("/api/v1/collections/{id}/breadcrumb");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Get collection breadcrumb")
      .WithDescription("Retrieves the full ancestor chain from root to current collection"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("id"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    var query = new GetCollectionBreadcrumbQuery
    {
      CollectionId = collectionId
    };

    try
    {
      var result = await _handler.Handle(query, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve breadcrumb" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
