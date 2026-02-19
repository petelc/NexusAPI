using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Handlers;
using Nexus.API.UseCases.Workspaces.Queries;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for searching workspaces by name
/// </summary>
public class SearchWorkspacesEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public SearchWorkspacesEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/workspaces/search");
    Roles("Viewer", "Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("SearchWorkspaces")
      .WithSummary("Searches workspaces by name")
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

      // Parse query parameters
      var searchTerm = Query<string>("searchTerm", isRequired: false);
      if (string.IsNullOrWhiteSpace(searchTerm))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Search term 'searchTerm' is required" }, ct);
        return;
      }

      var teamIdStr = Query<string?>("teamId", isRequired: false);
      Guid? teamId = null;
      if (!string.IsNullOrEmpty(teamIdStr) && Guid.TryParse(teamIdStr, out var parsedTeamId))
      {
        teamId = parsedTeamId;
      }

      // Create query
      var query = new SearchWorkspacesQuery(searchTerm, teamId);

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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to search workspaces" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
