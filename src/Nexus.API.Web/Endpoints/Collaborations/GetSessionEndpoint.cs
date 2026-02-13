using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: GET /api/v1/collaboration/sessions/{id}
/// Gets a collaboration session by ID
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetSessionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetSessionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/collaboration/sessions/{id}");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Get session by ID")
            .WithDescription("Retrieves detailed information about a collaboration session including all participants."));
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
            var query = new GetSessionByIdQuery
            {
                SessionId = SessionId.Create(sessionId)
            };
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Session not found" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve session" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
