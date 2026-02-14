using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: PUT /collections/{id}
/// Updates collection properties
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateCollectionEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public UpdateCollectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Put("/collections/{id}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Update collection")
      .WithDescription("Updates collection properties. Only provided fields are updated."));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("id"), out var collectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateCollectionRequestBody>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    var command = new UpdateCollectionCommand
    {
      CollectionId = collectionId,
      Name = request.Name,
      Description = request.Description,
      Icon = request.Icon,
      Color = request.Color
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to update collection" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class UpdateCollectionRequestBody
{
  public string? Name { get; set; }
  public string? Description { get; set; }
  public string? Icon { get; set; }
  public string? Color { get; set; }
}
