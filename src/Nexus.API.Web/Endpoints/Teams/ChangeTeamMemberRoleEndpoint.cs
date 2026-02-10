using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Teams.Handlers;


namespace Nexus.API.Web.Endpoints.Teams;

/// <summary>
/// Endpoint: PUT /api/v1/teams/{id}/members/{userId}/role
/// Changes a member's role
/// Requires: Editor, Admin roles
/// </summary>
public class ChangeTeamMemberRoleEndpoint : EndpointWithoutRequest
{
    private readonly ChangeTeamMemberRoleCommandHandler _handler;

    public ChangeTeamMemberRoleEndpoint(ChangeTeamMemberRoleCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Put("/api/v1/teams/{id}/members/{userId}/role");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Teams")
            .WithSummary("Change member role")
            .WithDescription("Changes a team member's role. Requires Owner or Admin role."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<ChangeRoleRequest>(ct);
        if (request == null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
            return;
        }

        try
        {
            var result = await _handler.Handle(teamId, targetUserId, request.NewRole, ct);

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
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Failed to change role" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}

public record ChangeRoleRequest(string NewRole);
