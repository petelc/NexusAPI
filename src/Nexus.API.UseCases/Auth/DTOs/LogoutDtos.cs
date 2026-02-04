namespace Nexus.API.UseCases.Auth.DTOs;

/// <summary>
/// Request to logout and invalidate refresh token
/// </summary>
public record LogoutRequestDto(
  string RefreshToken);
