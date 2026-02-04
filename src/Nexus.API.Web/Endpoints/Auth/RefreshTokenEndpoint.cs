using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Entities;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Refresh access token endpoint
/// POST /api/v1/auth/refresh
/// Implements token rotation for security
/// </summary>
public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequestDto, RefreshTokenResponseDto>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly IRefreshTokenRepository _refreshTokenRepository;

  public RefreshTokenEndpoint(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository)
  {
    _userManager = userManager;
    _jwtTokenService = jwtTokenService;
    _refreshTokenRepository = refreshTokenRepository;
  }

  public override void Configure()
  {
    Post("/auth/refresh");
    AllowAnonymous();

    Description(b => b
      .WithTags("Authentication")
      .WithSummary("Refresh access token")
      .WithDescription("Exchanges a valid refresh token for a new access token and refresh token. Implements token rotation for security."));
  }

  public override async Task HandleAsync(
    RefreshTokenRequestDto request,
    CancellationToken ct)
  {
    // Validate access token (without lifetime validation)
    var principal = _jwtTokenService.ValidateToken(request.AccessToken);
    if (principal == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid access token" }
      }, ct);
      return;
    }

    // Extract JTI from access token
    var jwtId = _jwtTokenService.GetJwtIdFromToken(request.AccessToken);
    if (string.IsNullOrEmpty(jwtId))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid token format" }
      }, ct);
      return;
    }

    // Get stored refresh token
    var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
      request.RefreshToken, ct);

    if (storedRefreshToken == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Refresh token not found" }
      }, ct);
      return;
    }

    // Validate refresh token
    if (!storedRefreshToken.IsValid())
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new 
        { 
          message = storedRefreshToken.IsExpired() 
            ? "Refresh token has expired" 
            : "Refresh token is no longer valid" 
        }
      }, ct);
      return;
    }

    // Verify JTI matches
    if (storedRefreshToken.JwtId != jwtId)
    {
      // Possible token theft - invalidate all user tokens
      await _refreshTokenRepository.InvalidateAllUserTokensAsync(
        storedRefreshToken.UserId,
        "Token mismatch detected - possible security breach",
        ct);

      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid token pair. All tokens have been invalidated for security." }
      }, ct);
      return;
    }

    // Mark old refresh token as used
    storedRefreshToken.MarkAsUsed();
    await _refreshTokenRepository.UpdateAsync(storedRefreshToken, ct);

    // Get user
    var userId = principal.FindFirst("uid")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid user claim" }
      }, ct);
      return;
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null || !user.IsActive)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "User not found or inactive" }
      }, ct);
      return;
    }

    // Generate new tokens
    var roles = await _userManager.GetRolesAsync(user);
    var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
    var newJwtId = _jwtTokenService.GetJwtIdFromToken(newAccessToken);
    var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

    // Store new refresh token
    var refreshTokenEntity = RefreshToken.Create(
      user.Id,
      newRefreshToken,
      newJwtId!,
      daysValid: 7);

    await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);

    var response = new RefreshTokenResponseDto(
      newAccessToken,
      newRefreshToken,
      DateTime.UtcNow.AddMinutes(15));

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}
