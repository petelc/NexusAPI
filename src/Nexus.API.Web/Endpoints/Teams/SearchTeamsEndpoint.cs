using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Handlers;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: GET /api/v1/teams/search?term={searchTerm}
/// Searches teams by name
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class SearchTeamsEndpoint : EndpointWithoutRequest
{
    private readonly SearchTeamsQueryHandler _handler;

    public SearchTeamsEndpoint(SearchTeamsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/api/v1/teams/search");
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
            var result = await _handler.Handle(searchTerm, ct);

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
