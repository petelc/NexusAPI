using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: POST /api/v1/collections
/// Creates a new collection
/// Requires: Editor, Admin roles
/// </summary>
public class CreateCollectionEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public CreateCollectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/collections");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Create a new collection")
      .WithDescription("Creates a new collection in the specified workspace. Can be root level or nested under a parent collection."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<CreateCollectionCommand>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    try
    {
      var result = await _mediator.Send(request, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 201;
        HttpContext.Response.Headers.Append("Location", $"/api/v1/collections/{result.Value.Collection.CollectionId}");
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Parent collection not found" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to create collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
