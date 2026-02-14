using MediatR;
using FastEndpoints;
using Nexus.API.UseCases.Collections.Queries;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/workspaces/{workspaceId}/collections/roots
/// Gets root-level collections in a workspace
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetRootCollectionsEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetRootCollectionsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/workspaces/{workspaceId}/collections/roots");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Get root collections")
      .WithDescription("Retrieves all root-level collections in a workspace"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("workspaceId"), out var workspaceId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid workspace ID" }, ct);
      return;
    }

    var query = new GetRootCollectionsQuery
    {
      WorkspaceId = workspaceId
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve root collections" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
