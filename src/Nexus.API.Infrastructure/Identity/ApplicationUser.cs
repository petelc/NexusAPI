using Microsoft.AspNetCore.Identity;

namespace Nexus.API.Infrastructure.Identity;

/// <summary>
/// Identity framework wrapper for authentication and authorization.
/// This is separate from the domain User aggregate - it only handles authentication concerns.
/// Links to domain via shared Guid ID.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
  // Basic user information
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  
  // Profile information
  public string? AvatarUrl { get; set; }
  public string? Bio { get; set; }
  public string? Title { get; set; }
  public string? Department { get; set; }
  
  // User preferences
  public string Theme { get; set; } = "Auto"; // Light, Dark, Auto
  public string Language { get; set; } = "en-US";
  public bool NotificationsEnabled { get; set; } = true;
  public string EmailDigest { get; set; } = "Weekly"; // Daily, Weekly, None
  
  // Additional tracking
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? LastLoginAt { get; set; }
  public bool IsActive { get; set; } = true;
}
