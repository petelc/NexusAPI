using FastEndpoints;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Logout endpoint
/// POST /api/v1/auth/logout
/// Invalidates refresh token
/// </summary>
public class LogoutEndpoint : Endpoint<LogoutRequestDto>
{
  private readonly IRefreshTokenRepository _refreshTokenRepository;

  public LogoutEndpoint(IRefreshTokenRepository refreshTokenRepository)
  {
    _refreshTokenRepository = refreshTokenRepository;
  }

  public override void Configure()
  {
    Post("/auth/logout");
    AllowAnonymous(); // Can logout without valid access token

    Description(b => b
      .WithTags("Authentication")
      .WithSummary("Logout user")
      .WithDescription("Invalidates the refresh token. Access tokens cannot be invalidated but will expire in 15 minutes."));
  }

  public override async Task HandleAsync(
    LogoutRequestDto request,
    CancellationToken ct)
  {
    var storedToken = await _refreshTokenRepository.GetByTokenAsync(
      request.RefreshToken, ct);

    if (storedToken != null && !storedToken.Invalidated)
    {
      storedToken.Invalidate("User logout");
      await _refreshTokenRepository.UpdateAsync(storedToken, ct);
    }

    HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
  }
}
