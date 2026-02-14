using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Permissions.Commands;

namespace Nexus.API.Web.Endpoints.Permissions;

/// <summary>
/// Endpoint: DELETE /api/v1/permissions/{id}
/// Revokes a permission grant by its ID.
/// The requesting user must be the original granter or hold Admin/Owner on the resource.
/// </summary>
public class RevokePermissionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public RevokePermissionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/permissions/{id}");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Permissions")
            .WithSummary("Revoke a permission")
            .WithDescription("Revokes a specific permission grant. Owner permissions cannot be revoked via this endpoint."));
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

        var permissionIdStr = Route<string>("id");
        if (!Guid.TryParse(permissionIdStr, out var permissionId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid permission ID" }, ct);
            return;
        }

        try
        {
            var command = new RevokePermissionCommand(permissionId, userId);
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 204;
                return;
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Permission not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Insufficient permissions to revoke this grant" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 422;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Validation failed" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to revoke permission" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
