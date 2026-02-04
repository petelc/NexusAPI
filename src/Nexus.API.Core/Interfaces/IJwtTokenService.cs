using System.Security.Claims;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// Implementation is in Infrastructure layer.
/// </summary>
public interface IJwtTokenService
{
  string GenerateAccessToken(object user, IList<string> roles);
  string GenerateRefreshToken();
  ClaimsPrincipal? ValidateToken(string token);
  string? GetJwtIdFromToken(string token);
}

public record TokenResult(
  string AccessToken,
  string RefreshToken,
  DateTime ExpiresAt);
