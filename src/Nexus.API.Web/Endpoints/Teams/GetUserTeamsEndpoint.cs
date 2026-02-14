using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Queries;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: GET /api/v1/teams/my
/// Gets all teams the current user is a member of
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetUserTeamsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetUserTeamsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/teams/my");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Get user's teams")
            .WithDescription("Retrieves all teams the current user is a member of."));
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

        try
        {
            var query = new GetUserTeamsQuery();
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve teams" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
