using MediatR;
using System.Security.Claims;
using FastEndpoints;
using Nexus.API.UseCases.Workspaces.Commands;
using Nexus.API.UseCases.Workspaces.Handlers;

namespace Nexus.API.Web.Endpoints.Workspaces;

/// <summary>
/// Endpoint for creating a new workspace
/// </summary>
public class CreateWorkspaceEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public CreateWorkspaceEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/workspaces");
    Roles("Editor", "Admin");
    Description(b => b
      .WithTags("Workspaces")
      .WithName("CreateWorkspace")
      .WithSummary("Creates a new workspace")
      .Produces(201)
      .Produces(400)
      .Produces(401)
      .Produces(409));
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

      // Read request body
      var request = await HttpContext.Request.ReadFromJsonAsync<CreateWorkspaceRequestBody>(ct);
      if (request == null)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
        return;
      }

      // Validate
      if (string.IsNullOrWhiteSpace(request.Name))
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Name is required" }, ct);
        return;
      }

      if (request.TeamId == Guid.Empty)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "TeamId is required" }, ct);
        return;
      }

      // Create command
      var command = new CreateWorkspaceCommand(
        request.Name,
        request.Description,
        request.TeamId);

      // Handle
      var result = await _mediator.Send(command, ct);

      if (result.IsSuccess)
      {
        HttpContext.Response.StatusCode = 201;
        HttpContext.Response.Headers.Append("Location", $"/api/v1/workspaces/{result.Value.WorkspaceId}");
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Validation failed" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
      {
        HttpContext.Response.StatusCode = 401;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      }
      else if (result.Status == Ardalis.Result.ResultStatus.Error && 
               result.Errors.Any(e => e.Contains("already exists")))
      {
        HttpContext.Response.StatusCode = 409;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() }, ct);
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to create workspace" }, ct);
      }
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}

public class CreateWorkspaceRequestBody
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid TeamId { get; set; }
}
