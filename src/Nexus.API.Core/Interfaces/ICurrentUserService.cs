using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Service to get the current authenticated user from HttpContext
/// </summary>
public interface ICurrentUserService
{
  /// <summary>
  /// Gets the current user's ID from claims
  /// </summary>
  UserId? UserId { get; }

  /// <summary>
  /// Gets the current user's email from claims
  /// </summary>
  Email? Email { get; }

  /// <summary>
  /// Gets the current user's username from claims
  /// </summary>
  string? Username { get; }

  /// <summary>
  /// Checks if a user is currently authenticated
  /// </summary>
  bool IsAuthenticated { get; }

  /// <summary>
  /// Gets the current user's ID or throws if not authenticated
  /// </summary>
  UserId GetRequiredUserId();
}
