using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: GET /api/v1/collaboration/sessions/active?resourceType={type}&amp;resourceId={id}
/// Gets active sessions for a resource
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetActiveSessionsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetActiveSessionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/collaboration/sessions/active");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Get active sessions for a resource")
            .WithDescription("Retrieves all active collaboration sessions for a specific document or diagram."));
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

        var resourceTypeStr = HttpContext.Request.Query["resourceType"].ToString();
        var resourceIdStr = HttpContext.Request.Query["resourceId"].ToString();

        if (string.IsNullOrEmpty(resourceTypeStr) || string.IsNullOrEmpty(resourceIdStr))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Both resourceType and resourceId are required" }, ct);
            return;
        }

        if (!Guid.TryParse(resourceIdStr, out var resourceId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid resourceId format" }, ct);
            return;
        }

        try
        {
            var query = new GetActiveSessionsQuery
            {
                ResourceType = resourceTypeStr,
                ResourceId = ResourceId.Create(resourceId)
            };
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid resource type" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve sessions" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
