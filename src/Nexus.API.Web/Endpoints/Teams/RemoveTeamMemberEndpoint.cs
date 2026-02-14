using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Handlers;
using Nexus.API.UseCases.Teams.Commands;

namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: DELETE /api/v1/teams/{id}/members/{userId}
/// Removes a member from a team
/// Requires: Viewer, Editor, Admin roles (can remove self, or Owner/Admin can remove others)
/// </summary>
public class RemoveTeamMemberEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public RemoveTeamMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/teams/{id}/members/{userId}");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Remove a team member")
            .WithDescription("Removes a member from the team. Members can remove themselves, or Owner/Admin can remove others."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserIdClaim = User.FindFirstValue("uid");
        if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            HttpContext.Response.StatusCode = 401;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
            return;
        }

        var teamIdStr = Route<string>("id");
        var targetUserIdStr = Route<string>("userId");

        if (!Guid.TryParse(teamIdStr, out var teamId) || !Guid.TryParse(targetUserIdStr, out var targetUserId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid team ID or user ID" }, ct);
            return;
        }

        try
        {
            var command = new RemoveTeamMemberCommand(teamId, targetUserId);
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 204;
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
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to remove member" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
