using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Enable 2FA for user account
/// POST /api/v1/auth/2fa/enable
/// Requires authentication
/// </summary>
public class Enable2FAEndpoint : EndpointWithoutRequest<Enable2FAResponseDto>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly UrlEncoder _urlEncoder;

  public Enable2FAEndpoint(
    UserManager<ApplicationUser> userManager,
    UrlEncoder urlEncoder)
  {
    _userManager = userManager;
    _urlEncoder = urlEncoder;
  }

  public override void Configure()
  {
    Post("/auth/2fa/enable");
    Roles("Viewer", "Editor", "Admin"); // Requires authentication

    Description(b => b
      .WithTags("Authentication", "2FA")
      .WithSummary("Enable two-factor authentication")
      .WithDescription("Generates a shared key and QR code for setting up 2FA with an authenticator app."));
  }

  public override async Task HandleAsync(CancellationToken ct)
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

    // Reset authenticator key to generate new one
    await _userManager.ResetAuthenticatorKeyAsync(user);
    var key = await _userManager.GetAuthenticatorKeyAsync(user);

    if (string.IsNullOrEmpty(key))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Failed to generate authenticator key" }
      }, ct);
      return;
    }

    // Generate QR code URI
    var authenticatorUri = GenerateQrCodeUri(user.Email!, key);

    // Generate QR code URL (using a QR code API service)
    var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(authenticatorUri)}";

    var response = new Enable2FAResponseDto(
      FormatKey(key),
      authenticatorUri,
      qrCodeUrl);

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }

  private string GenerateQrCodeUri(string email, string unformattedKey)
  {
    const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    
    return string.Format(
      AuthenticatorUriFormat,
      _urlEncoder.Encode("Nexus"),
      _urlEncoder.Encode(email),
      unformattedKey);
  }

  private static string FormatKey(string unformattedKey)
  {
    var result = new StringBuilder();
    int currentPosition = 0;
    
    while (currentPosition + 4 < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
      currentPosition += 4;
    }
    
    if (currentPosition < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition));
    }

    return result.ToString().ToLowerInvariant();
  }
}
