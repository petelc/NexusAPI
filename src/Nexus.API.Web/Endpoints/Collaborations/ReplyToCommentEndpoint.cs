using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/comments/{id}/replies
/// Replies to an existing comment
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class ReplyToCommentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ReplyToCommentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/comments/{id}/replies");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Reply to a comment")
            .WithDescription("Creates a threaded reply to an existing comment."));
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
        if (!Guid.TryParse(commentIdStr, out var parentCommentId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid comment ID" }, ct);
            return;
        }

        var request = await HttpContext.Request.ReadFromJsonAsync<ReplyToCommentRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        var command = new AddReplyCommand(
            CommentId.Create(parentCommentId),
            UserId.Create(userId),
            request.Text);

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 201;
                HttpContext.Response.Headers.Append("Location", $"/api/v1/collaboration/comments/{result.Value.CommentId}");
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Parent comment not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid reply data" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to add reply" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
