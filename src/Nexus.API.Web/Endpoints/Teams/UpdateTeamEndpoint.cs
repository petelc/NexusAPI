using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Commands;
using Nexus.API.UseCases.Teams.Handlers;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: PUT /api/v1/teams/{id}
/// Updates a team
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateTeamEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public UpdateTeamEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/teams/{id}");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Update a team")
            .WithDescription("Updates team name and/or description. Requires Owner or Admin role."));
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

        var teamIdStr = Route<string>("id");
        if (!Guid.TryParse(teamIdStr, out var teamId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid team ID" }, ct);
            return;
        }

        var request = await HttpContext.Request.ReadFromJsonAsync<UpdateTeamCommand>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        // Override teamId from route
        var command = new UpdateTeamCommand(teamId, request.Name, request.Description);

        try
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 401;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Team not found" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to update team" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
