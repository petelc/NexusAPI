using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Handlers;
using Nexus.API.UseCases.Workspaces.Queries;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for getting a workspace by ID
/// </summary>
public class GetWorkspaceByIdEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetWorkspaceByIdEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/workspaces/{id}");
    Roles("Viewer", "Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("GetWorkspaceById")
      .WithSummary("Gets a workspace by ID")
      .Produces(200)
      .Produces(401)
      .Produces(403)
      .Produces(404));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    try
    {
      // Authorize
      var userIdClaim = User.FindFirstValue("uid");
      if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
        return;
      }

      // Parse route parameter
      var idStr = Route<string>("id");
      if (!Guid.TryParse(idStr, out var workspaceId))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid workspace ID" }, ct);
        return;
      }

      // Parse query parameter
      var includeMembers = Query<bool?>("includeMembers", isRequired: false) ?? false;

      // Create query
      var query = new GetWorkspaceByIdQuery(workspaceId, includeMembers);

      // Handle
      var result = await _mediator.Send(query, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Workspace not found" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
      {
        HttpContext.Response.StatusCode = 403;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Forbidden" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to get workspace" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
