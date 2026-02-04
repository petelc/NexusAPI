using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Disable 2FA for user account
/// POST /api/v1/auth/2fa/disable
/// Requires authentication and password confirmation
/// </summary>
public class Disable2FAEndpoint : Endpoint<Disable2FARequestDto>
{
  private readonly UserManager<ApplicationUser> _userManager;

  public Disable2FAEndpoint(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/auth/2fa/disable");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Authentication", "2FA")
      .WithSummary("Disable two-factor authentication")
      .WithDescription("Disables 2FA for the user account. Requires password confirmation for security."));
  }

  public override async Task HandleAsync(
    Disable2FARequestDto request,
    CancellationToken ct)
  {
    var userId = User.FindFirstValue("uid");
    if (string.IsNullOrEmpty(userId))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "User not authenticated" }
      }, ct);
      return;
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "User not found" }
      }, ct);
      return;
    }

    // Verify password
    var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    if (!passwordValid)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid password" }
      }, ct);
      return;
    }

    // Disable 2FA
    await _userManager.SetTwoFactorEnabledAsync(user, false);

    // Reset authenticator key
    await _userManager.ResetAuthenticatorKeyAsync(user);

    HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
  }
}
