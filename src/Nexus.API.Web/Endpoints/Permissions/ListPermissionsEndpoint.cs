using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Permissions.Queries;

namespace Nexus.API.Web.Endpoints.Permissions;

/// <summary>
/// Endpoint: GET /api/v1/permissions?resourceType={type}&amp;resourceId={id}
/// Lists all permission grants for a specific resource.
/// The requesting user must hold at least Viewer access on the resource.
/// </summary>
public class ListPermissionsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListPermissionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/permissions");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Permissions")
            .WithSummary("List permissions for a resource")
            .WithDescription("Returns all permission grants on a given resource. Requires at least Viewer access on the resource."));
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

        var resourceType = HttpContext.Request.Query["resourceType"].ToString();
        var resourceIdStr = HttpContext.Request.Query["resourceId"].ToString();

        if (string.IsNullOrWhiteSpace(resourceType))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "resourceType query parameter is required" }, ct);
            return;
        }

        if (!Guid.TryParse(resourceIdStr, out var resourceId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "resourceId must be a valid GUID" }, ct);
            return;
        }

        try
        {
            var query = new ListPermissionsQuery(resourceType, resourceId, userId);
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(new { data = result.Value }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Insufficient permissions to view grants on this resource" }, ct);
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
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to list permissions" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
