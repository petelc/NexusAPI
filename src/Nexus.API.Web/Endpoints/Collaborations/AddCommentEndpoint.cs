using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.DTOs;
using Nexus.API.UseCases.Collaboration.Handlers;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/comments
/// Adds a new comment to a resource
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class AddCommentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public AddCommentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/comments");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration - Comments")
            .WithSummary("Add a comment")
            .WithDescription("Adds a new comment to a document or diagram. Can optionally specify a position for inline comments."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<AddCommentRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        var command = new AddCommentCommand(
            request.ResourceType,
            request.SessionId.HasValue ? SessionId.Create(request.SessionId.Value) : null,
            ResourceId.Create(request.ResourceId),
            UserId.Create(userId),
            request.Text,
            request.Position);

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 201;
                HttpContext.Response.Headers.Append("Location", $"/api/v1/collaboration/comments/{result.Value.CommentId}");
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid comment data" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to add comment" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
