using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.Handlers;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for deleting a workspace
/// </summary>
public class DeleteWorkspaceEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public DeleteWorkspaceEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Delete("/workspaces/{id}");
    Roles("Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("DeleteWorkspace")
      .WithSummary("Deletes a workspace (soft delete)")
      .Produces(204)
      .Produces(400)
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

      // Create command
      var command = new DeleteWorkspaceCommand(workspaceId);

      // Handle
      var result = await _mediator.Send(command, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 204;
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Forbidden - Only owners can delete workspaces" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to delete workspace" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
