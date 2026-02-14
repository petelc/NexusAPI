using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Commands;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for adding a member to a workspace
/// </summary>
public class AddMemberEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public AddMemberEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/workspaces/{id}/members");
    Roles("Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("AddWorkspaceMember")
      .WithSummary("Adds a member to a workspace")
      .Produces(201)
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

      // Parse route parameter
      var idStr = Route<string>("id");
      if (!Guid.TryParse(idStr, out var workspaceId))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid workspace ID" }, ct);
        return;
      }

      // Read request body
      var request = await HttpContext.Request.ReadFromJsonAsync<AddMemberRequestBody>(ct);
      if (request == null)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
        return;
      }

      // Validate
      if (request.UserId == Guid.Empty)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "UserId is required" }, ct);
        return;
      }

      if (string.IsNullOrWhiteSpace(request.Role))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Role is required" }, ct);
        return;
      }

      // Create command
      var command = new AddMemberCommand(workspaceId, request.UserId, request.Role);

      // Handle
      var result = await _mediator.Send(command, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 201;
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
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Forbidden - Only admins and owners can add members" }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to add member" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class AddMemberRequestBody
{
  public Guid UserId { get; set; }
  public string Role { get; set; } = string.Empty;
}
