namespace Nexus.API.UseCases.Auth.DTOs;

/// <summary>
/// Response with 2FA setup information
/// Contains QR code data and manual entry key
/// </summary>
public record Enable2FAResponseDto(
  string SharedKey,
  string AuthenticatorUri,
  string QrCodeSetupImageUrl);

/// <summary>
/// Request to verify and complete 2FA setup
/// </summary>
public record Verify2FASetupRequestDto(
  string Code);

/// <summary>
/// Request to login with 2FA code
/// Used after initial email/password verification
/// </summary>
public record TwoFactorLoginRequestDto(
  string Email,
  string Code);

/// <summary>
/// Request to disable 2FA
/// </summary>
public record Disable2FARequestDto(
  string Password);

/// <summary>
/// Response with recovery codes
/// Show once and user must save them
/// </summary>
public record RecoveryCodesResponseDto(
  IEnumerable<string> RecoveryCodes);
