using MediatR;
using FastEndpoints;
using Nexus.API.UseCases.Collections.Queries;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/collections/{parentId}/children
/// Gets child collections of a parent
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetChildCollectionsEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetChildCollectionsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/collections/{parentId}/children");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Get child collections")
      .WithDescription("Retrieves all direct children of a parent collection"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("parentId"), out var parentId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid parent collection ID" }, ct);
      return;
    }

    var query = new GetChildCollectionsQuery
    {
      ParentCollectionId = parentId
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve child collections" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
