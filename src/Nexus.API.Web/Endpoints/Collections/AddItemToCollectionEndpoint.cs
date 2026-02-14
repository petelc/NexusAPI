using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: POST /api/v1/collections/{collectionId}/items
/// Adds an item to a collection
/// Requires: Editor, Admin roles
/// </summary>
public class AddItemToCollectionEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public AddItemToCollectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/collections/{collectionId}/items");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Collections", "Items")
      .WithSummary("Add item to collection")
      .WithDescription("Adds a document, diagram, snippet, or sub-collection to a collection"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userIdClaim = User.FindFirstValue("uid");
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      return;
    }

    if (!Guid.TryParse(Route<string>("collectionId"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<AddItemRequestBody>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    var command = new AddItemToCollectionCommand
    {
      CollectionId = collectionId,
      ItemType = request.ItemType,
      ItemReferenceId = request.ItemReferenceId
    };

    try
    {
      var result = await _mediator.Send(command, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 201;
        HttpContext.Response.Headers.Append("Location", $"/api/v1/collections/{collectionId}");
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Collection not found" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to add item to collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class AddItemRequestBody
{
  public string ItemType { get; set; } = string.Empty;
  public Guid ItemReferenceId { get; set; }
}
