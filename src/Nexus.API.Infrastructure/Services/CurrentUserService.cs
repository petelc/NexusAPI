using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nexus.API.Core.Interfaces;
using UserIdValueObject = Nexus.API.Core.ValueObjects.UserId;
using EmailValueObject = Nexus.API.Core.ValueObjects.Email;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Service to get the current authenticated user from HttpContext
/// </summary>
public class CurrentUserService : ICurrentUserService
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CurrentUserService(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public UserIdValueObject? UserId
  {
    get
    {
      // Try "uid" custom claim first, then fall back to "sub" (JWT standard)
      var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

      if (string.IsNullOrWhiteSpace(userIdClaim))
        return null;

      if (Guid.TryParse(userIdClaim, out var guid))
        return UserIdValueObject.From(guid);

      return null;
    }
  }

  public EmailValueObject? Email
  {
    get
    {
      var emailClaim = _httpContextAccessor.HttpContext?.User?
        .FindFirst("email")?.Value;

      if (string.IsNullOrWhiteSpace(emailClaim))
        return null;

      return new EmailValueObject(emailClaim);
    }
  }

  public string? Username
  {
    get
    {
      return _httpContextAccessor.HttpContext?.User?
        .FindFirst("name")?.Value;
    }
  }

  public bool IsAuthenticated =>
    _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

  public UserIdValueObject GetRequiredUserId()
  {
    var userId = UserId;

    if (userId == null)
      throw new UnauthorizedAccessException("User is not authenticated");

    return userId ?? throw new UnauthorizedAccessException("User is not authenticated");
  }
}
