using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Queries;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/collections/{id}
/// Retrieves a specific collection
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetCollectionByIdEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetCollectionByIdEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/collections/{id}");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Get collection by ID")
      .WithDescription("Retrieves a specific collection with its items and metadata"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("id"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    var includeItems = Query<bool?>("includeItems") ?? true;

    var query = new GetCollectionByIdQuery
    {
      CollectionId = collectionId,
      IncludeItems = includeItems
    };

    try
    {
      var result = await _mediator.Send(query, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Collection not found" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
