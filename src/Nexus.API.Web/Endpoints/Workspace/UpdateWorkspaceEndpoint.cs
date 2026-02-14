using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.Handlers;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for updating a workspace
/// </summary>
public class UpdateWorkspaceEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public UpdateWorkspaceEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Put("/workspaces/{id}");
    Roles("Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("UpdateWorkspace")
      .WithSummary("Updates a workspace")
      .Produces(200)
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

      // Read request body
      var request = await HttpContext.Request.ReadFromJsonAsync<UpdateWorkspaceRequestBody>(ct);
      if (request == null)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
        return;
      }

      // Create command
      var command = new UpdateWorkspaceCommand(
        workspaceId,
        request.Name,
        request.Description);

      // Handle
      var result = await _mediator.Send(command, ct);

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
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to update workspace" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class UpdateWorkspaceRequestBody
{
  public string? Name { get; set; }
  public string? Description { get; set; }
}
