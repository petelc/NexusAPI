namespace Nexus.API.UseCases.Auth.DTOs;

public record AuthResponseDto(
  string AccessToken,
  string RefreshToken,
  DateTime ExpiresAt,
  UserDto User);

public record UserDto(
  Guid UserId,
  string Email,
  string Username,
  string FirstName,
  string LastName,
  string? AvatarUrl,
  bool EmailConfirmed,
  bool TwoFactorEnabled);

public record RegisterRequestDto(
  string Email,
  string Username,
  string FirstName,
  string LastName,
  string Password,
  string ConfirmPassword);

public record LoginRequestDto(
  string Email,
  string Password,
  bool RememberMe = false);
