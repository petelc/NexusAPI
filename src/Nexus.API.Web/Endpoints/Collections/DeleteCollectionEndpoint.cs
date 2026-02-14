using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: DELETE /api/v1/collections/{id}
/// Soft deletes a collection
/// Requires: Admin role
/// </summary>
public class DeleteCollectionEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public DeleteCollectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Delete("/collections/{id}");
    Roles("Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Delete collection (soft delete)")
      .WithDescription("Soft deletes a collection. Must be empty unless Force=true"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("id"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    var force = Query<bool?>("force") ?? false;

    var command = new DeleteCollectionCommand
    {
      CollectionId = collectionId,
      Force = force
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Collection not found" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to delete collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
