using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Commands;
using Nexus.API.UseCases.Teams.Handlers;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: POST /api/v1/teams
/// Creates a new team
/// Requires: Editor, Admin roles
/// </summary>
public class CreateTeamEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public CreateTeamEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/teams");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Create a new team")
            .WithDescription("Creates a new team. The creator automatically becomes the team owner."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<CreateTeamCommand>(ct);
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
                HttpContext.Response.Headers.Append("Location", $"/api/v1/teams/{result.Value.TeamId}");
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 401;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to create team" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
