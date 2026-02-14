using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: DELETE /api/v1/collections/{collectionId}/items/{itemReferenceId}
/// Removes an item from a collection
/// Requires: Editor, Admin roles
/// </summary>
public class RemoveItemFromCollectionEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public RemoveItemFromCollectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Delete("/collections/{collectionId}/items/{itemReferenceId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Collections", "Items")
      .WithSummary("Remove item from collection")
      .WithDescription("Removes an item from a collection. The item itself is not deleted, only the association."));
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

    var command = new RemoveItemFromCollectionCommand
    {
      CollectionId = collectionId,
      ItemReferenceId = itemReferenceId
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to remove item from collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
