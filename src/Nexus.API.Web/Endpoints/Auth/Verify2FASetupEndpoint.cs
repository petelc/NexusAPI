using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Verify 2FA setup with code from authenticator app
/// POST /api/v1/auth/2fa/verify
/// Requires authentication
/// </summary>
public class Verify2FASetupEndpoint : Endpoint<Verify2FASetupRequestDto, RecoveryCodesResponseDto>
{
  private readonly UserManager<ApplicationUser> _userManager;

  public Verify2FASetupEndpoint(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/auth/2fa/verify");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Authentication", "2FA")
      .WithSummary("Verify 2FA setup")
      .WithDescription("Verifies the 2FA code from authenticator app and completes setup. Returns recovery codes that must be saved."));
  }

  public override async Task HandleAsync(
    Verify2FASetupRequestDto request,
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

    // Verify the code
    var verificationCode = request.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
    var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
      user,
      _userManager.Options.Tokens.AuthenticatorTokenProvider,
      verificationCode);

    if (!is2faTokenValid)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid verification code" }
      }, ct);
      return;
    }

    // Enable 2FA
    await _userManager.SetTwoFactorEnabledAsync(user, true);

    // Generate recovery codes
    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

    var response = new RecoveryCodesResponseDto(recoveryCodes!);

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}
