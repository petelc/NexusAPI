using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: PUT /api/v1/collaboration/comments/{id}
/// Updates an existing comment
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class UpdateCommentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public UpdateCommentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/collaboration/comments/{id}");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Update a comment")
            .WithDescription("Updates the text of an existing comment. Only the comment author can update it."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<UpdateCommentRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        var command = new UpdateCommentCommand(
            CommentId.Create(commentId),
            UserId.Create(userId),
            request.Text);

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You can only update your own comments" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Comment not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid comment data" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to update comment" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
