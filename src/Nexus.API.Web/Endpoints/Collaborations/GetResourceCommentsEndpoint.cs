using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Queries;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: GET /api/v1/collaboration/comments?resourceType={type}&amp;resourceId={id}&amp;includeDeleted={true|false}
/// Gets comments for a specific resource
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetResourceCommentsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetResourceCommentsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/collaboration/comments");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Get resource comments")
            .WithDescription("Retrieves all comments for a specific document or diagram."));
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
        var includeDeletedStr = HttpContext.Request.Query["includeDeleted"].ToString();

        if (string.IsNullOrEmpty(resourceType) || string.IsNullOrEmpty(resourceIdStr))
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

        var includeDeleted = !string.IsNullOrEmpty(includeDeletedStr) &&
                            bool.TryParse(includeDeletedStr, out var result) && result;

        var query = new GetResourceCommentsQuery(resourceType, ResourceId.Create(resourceId), includeDeleted);

        try
        {
            var queryResult = await _mediator.Send(query, ct);

            if (queryResult.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(queryResult.Value, ct);
            }
            else if (queryResult.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = queryResult.Errors.FirstOrDefault() ?? "Invalid resource type" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = queryResult.Errors.FirstOrDefault() ?? "Failed to retrieve comments" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
