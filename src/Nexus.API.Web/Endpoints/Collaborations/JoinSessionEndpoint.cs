using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Handlers;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/sessions/{id}/join
/// Joins an existing collaboration session
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class JoinSessionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public JoinSessionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/sessions/{id}/join");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Join a collaboration session")
            .WithDescription("Joins an existing collaboration session with the specified role (Viewer or Editor)."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<JoinSessionRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        // Create command with route sessionId
        var command = new JoinSessionCommand
        {
            SessionId = SessionId.Create(sessionId),
            UserId = ParticipantId.Create(userId),
            Role = request.Role
        };

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Session not found or not active" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid role specified" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to join session" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}

public record JoinSessionRequest(string Role);
