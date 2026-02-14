using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: PUT /api/v1/collections/{collectionId}/items/{itemReferenceId}/order
/// Reorders an item within a collection
/// Requires: Editor, Admin roles
/// </summary>
public class ReorderItemEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public ReorderItemEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Put("/collections/{collectionId}/items/{itemReferenceId}/order");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Collections", "Items")
      .WithSummary("Reorder item in collection")
      .WithDescription("Changes the order of an item within a collection"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("collectionId"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    if (!Guid.TryParse(Route<string>("itemReferenceId"), out var itemReferenceId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid item reference ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<ReorderItemRequestBody>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    var command = new ReorderItemCommand
    {
      CollectionId = collectionId,
      ItemReferenceId = itemReferenceId,
      NewOrder = request.NewOrder
    };

    try
    {
      var result = await _mediator.Send(command, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Collection or item not found" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to reorder item" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class ReorderItemRequestBody
{
  public int NewOrder { get; set; }
}
