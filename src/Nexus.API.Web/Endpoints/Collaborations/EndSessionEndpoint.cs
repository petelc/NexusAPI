using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/sessions/{id}/end
/// Ends a collaboration session
/// Requires: Editor, Admin roles
/// </summary>
public class EndSessionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public EndSessionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/sessions/{id}/end");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("End a collaboration session")
            .WithDescription("Ends an active collaboration session. Only active participants can end the session."));
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
            var command = new EndSessionCommand
            {
                SessionId = SessionId.Create(sessionId),
                UserId = ParticipantId.Create(userId)
            };
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(new { message = "Session ended successfully" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You are not authorized to end this session" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Session not found" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to end session" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
