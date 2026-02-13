using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Collaboration.Commands;
using Nexus.API.UseCases.Collaboration.Handlers;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: POST /api/v1/collaboration/sessions
/// Starts a new collaboration session
/// Requires: Editor, Admin roles
/// </summary>
public class StartSessionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public StartSessionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/collaboration/sessions");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Start a collaboration session")
            .WithDescription("Starts a new collaboration session on a document or diagram. The initiator is automatically added as the first participant."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<StartSessionCommand>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        try
        {
            var result = await _mediator.Send(request, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 201;
                HttpContext.Response.Headers.Append("Location", $"/api/v1/collaboration/sessions/{result.Value.SessionId}");
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Invalid session data" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Conflict)
            {
                HttpContext.Response.StatusCode = 409;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "An active session already exists for this resource" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to start session" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
