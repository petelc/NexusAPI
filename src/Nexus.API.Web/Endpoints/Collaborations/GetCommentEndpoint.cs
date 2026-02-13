using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Queries;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: GET /api/v1/collaboration/comments/{id}
/// Gets a comment by ID
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetCommentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetCommentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/collaboration/comments/{id}");
        Roles("Viewer", "Editor, Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Get comment by ID")
            .WithDescription("Retrieves detailed information about a comment including all replies."));
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

        var commentIdStr = Route<string>("id");
        if (!Guid.TryParse(commentIdStr, out var commentId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid comment ID" }, ct);
            return;
        }

        var query = new GetCommentByIdQuery(CommentId.Create(commentId));

        try
        {
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Comment not found" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve comment" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
