using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Commands;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/sessions/{id}/leave
/// Leaves a collaboration session
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class LeaveSessionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public LeaveSessionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/sessions/{id}/leave");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Leave a collaboration session")
            .WithDescription("Leaves an active collaboration session. If the last participant leaves, the session will be automatically ended."));
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

        var sessionIdStr = Route<string>("id");
        if (!Guid.TryParse(sessionIdStr, out var sessionId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid session ID" }, ct);
            return;
        }

        try
        {
            var command = new LeaveSessionCommand
            {
                SessionId = SessionId.Create(sessionId),
                UserId = ParticipantId.Create(userId)
            };
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(new { message = "Left session successfully" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Session not found or you are not a participant" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to leave session" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
