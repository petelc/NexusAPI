using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Queries;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: GET /api/v1/teams/search?term={searchTerm}
/// Searches teams by name
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class SearchTeamsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public SearchTeamsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/teams/search");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Search teams")
            .WithDescription("Searches teams by name. Returns only teams the user is a member of."));
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

        var searchTerm = HttpContext.Request.Query["term"].ToString();

        try
        {
            var query = new SearchTeamsQuery(searchTerm);
            var result = await _mediator.Send(query, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to search teams" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
