using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Handlers;
using Nexus.API.UseCases.Workspaces.Queries;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for getting all workspaces for a team
/// </summary>
public class GetTeamWorkspacesEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetTeamWorkspacesEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/teams/{teamId}/workspaces");
    Roles("Viewer", "Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("GetTeamWorkspaces")
      .WithSummary("Gets all workspaces for a team")
      .Produces(200)
      .Produces(400)
      .Produces(401));
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
      var teamIdStr = Route<string>("teamId");
      if (!Guid.TryParse(teamIdStr, out var teamId))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid team ID" }, ct);
        return;
      }

      // Create query
      var query = new GetTeamWorkspacesQuery(teamId);

      // Handle
      var result = await _mediator.Send(query, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to get team workspaces" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
