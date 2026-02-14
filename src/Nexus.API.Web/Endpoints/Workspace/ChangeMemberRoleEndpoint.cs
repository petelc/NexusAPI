using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.Handlers;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for changing a member's role in a workspace
/// </summary>
public class ChangeMemberRoleEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public ChangeMemberRoleEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Put("/workspaces/{id}/members/{userId}/role");
    Roles("Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("ChangeMemberRole")
      .WithSummary("Changes a member's role in a workspace")
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
      if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
        return;
      }

      // Parse route parameters
      var idStr = Route<string>("id");
      if (!Guid.TryParse(idStr, out var workspaceId))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid workspace ID" }, ct);
        return;
      }

      var userIdStr = Route<string>("userId");
      if (!Guid.TryParse(userIdStr, out var userId))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid user ID" }, ct);
        return;
      }

      // Read request body
      var request = await HttpContext.Request.ReadFromJsonAsync<ChangeMemberRoleRequestBody>(ct);
      if (request == null)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
        return;
      }

      // Validate
      if (string.IsNullOrWhiteSpace(request.NewRole))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "NewRole is required" }, ct);
        return;
      }

      // Create command
      var command = new ChangeMemberRoleCommand(workspaceId, userId, request.NewRole);

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
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Workspace or member not found" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
      {
        HttpContext.Response.StatusCode = 403;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Forbidden - Only admins and owners can change roles" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to change member role" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class ChangeMemberRoleRequestBody
{
  public string NewRole { get; set; } = string.Empty;
}
