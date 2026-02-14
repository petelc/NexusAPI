using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Permissions.Commands;
using Nexus.API.UseCases.Permissions.DTOs;

namespace Nexus.API.Web.Endpoints.Permissions;

/// <summary>
/// Endpoint: POST /api/v1/permissions
/// Grants a permission to a user on a resource.
/// Requires: Admin role (system-level) — resource-level enforcement is in the handler.
/// </summary>
public class GrantPermissionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GrantPermissionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/permissions");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Permissions")
            .WithSummary("Grant a permission")
            .WithDescription("Grants a user a specific permission level on a Document, Diagram, or CodeSnippet. If the user already has a permission on the resource, it is updated."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue("uid");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
            return;
        }

        var request = await HttpContext.Request.ReadFromJsonAsync<GrantPermissionRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        var command = new GrantPermissionCommand(
            ResourceType: request.ResourceType,
            ResourceId: request.ResourceId,
            TargetUserId: request.UserId,
            Level: request.PermissionLevel,
            GrantedByUserId: userId,
            ExpiresAt: request.ExpiresAt);

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 201;
                HttpContext.Response.Headers.Append("Location", $"/api/v1/permissions/{result.Value.PermissionId}");
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 422;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Validation failed" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Insufficient permissions" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to grant permission" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
