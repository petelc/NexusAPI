using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Get current authenticated user info
/// GET /api/v1/auth/me
/// </summary>
public class MeEndpoint : EndpointWithoutRequest
{
  private readonly UserManager<ApplicationUser> _userManager;

  public MeEndpoint(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/auth/me");
    Roles("Viewer", "Editor", "Admin", "Guest");
    Description(b => b
      .WithTags("Authentication")
      .WithName("GetCurrentUser")
      .WithSummary("Get current authenticated user info")
      .Produces(200)
      .Produces(401));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userIdClaim = User.FindFirst("uid")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
      return;
    }

    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
      return;
    }

    var roles = await _userManager.GetRolesAsync(user);

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(new
    {
      user = new UserDto(
        user.Id,
        user.Email!,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.AvatarUrl,
        user.EmailConfirmed,
        user.TwoFactorEnabled),
      roles = roles
    }, ct);
  }
}
