using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Collaboration.Queries;

namespace Nexus.API.Web.Endpoints.Collaboration;

/// <summary>
/// Endpoint: GET /api/v1/collaboration/sessions/my?activeOnly={true|false}
/// Gets sessions for the current user
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetUserSessionsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetUserSessionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/collaboration/sessions/my");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Collaboration")
            .WithSummary("Get user's sessions")
            .WithDescription("Retrieves all collaboration sessions the current user is participating in. Can be filtered to active sessions only."));
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

        var activeOnlyStr = HttpContext.Request.Query["activeOnly"].ToString();
        var activeOnly = !string.IsNullOrEmpty(activeOnlyStr) && bool.TryParse(activeOnlyStr, out var result) && result;

        try
        {
            var query = new GetUserSessionsQuery
            {
                UserId = ParticipantId.Create(userId),
                ActiveOnly = activeOnly
            };
            var queryResult = await _mediator.Send(query, ct);

            if (queryResult.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(queryResult.Value, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = queryResult.Errors.FirstOrDefault() ?? "Failed to retrieve sessions" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
