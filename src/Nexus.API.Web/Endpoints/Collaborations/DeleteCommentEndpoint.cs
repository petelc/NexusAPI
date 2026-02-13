using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: DELETE /api/v1/collaboration/comments/{id}
/// Deletes a comment (soft delete)
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class DeleteCommentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeleteCommentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/collaboration/comments/{id}");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Delete a comment")
            .WithDescription("Soft deletes a comment. Only the comment author can delete it. Deleted comments are marked but remain visible."));
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

        var command = new DeleteCommentCommand(CommentId.Create(commentId), UserId.Create(userId));

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 204;
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You can only delete your own comments" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Comment not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid operation" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to delete comment" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
