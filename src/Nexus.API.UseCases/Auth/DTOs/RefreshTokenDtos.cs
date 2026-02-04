namespace Nexus.API.UseCases.Auth.DTOs;

/// <summary>
/// Request to refresh access token using refresh token
/// </summary>
public record RefreshTokenRequestDto(
  string AccessToken,
  string RefreshToken);

/// <summary>
/// Response after successful token refresh
/// Returns new access token and refresh token (rotation)
/// </summary>
public record RefreshTokenResponseDto(
  string AccessToken,
  string RefreshToken,
  DateTime ExpiresAt);
