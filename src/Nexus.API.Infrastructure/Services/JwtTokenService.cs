using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;

namespace Nexus.API.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
  private readonly IConfiguration _configuration;

  public JwtTokenService(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public string GenerateAccessToken(object user, IList<string> roles)
  {
    if (user is not ApplicationUser appUser)
      throw new ArgumentException("User must be ApplicationUser", nameof(user));

    var securityKey = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var jwtId = Guid.NewGuid().ToString();

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, appUser.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, appUser.Email!),
      new(JwtRegisteredClaimNames.Name, appUser.UserName!),
      new(JwtRegisteredClaimNames.GivenName, appUser.FirstName),
      new(JwtRegisteredClaimNames.FamilyName, appUser.LastName),
      new(JwtRegisteredClaimNames.Jti, jwtId),
      new("uid", appUser.Id.ToString()),
    };

    // Add roles as claims
    foreach (var role in roles)
    {
      claims.Add(new Claim("role", role));
    }

    var token = new JwtSecurityToken(
      issuer: _configuration["Jwt:Issuer"],
      audience: _configuration["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(15), // 15 minute access token
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  public ClaimsPrincipal? ValidateToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

    try
    {
      var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = _configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = _configuration["Jwt:Audience"],
        ValidateLifetime = false, // Don't validate lifetime for refresh token validation
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      return principal;
    }
    catch
    {
      return null;
    }
  }

  public string? GetJwtIdFromToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    
    try
    {
      var jwtToken = tokenHandler.ReadJwtToken(token);
      return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
    }
    catch
    {
      return null;
    }
  }
}
