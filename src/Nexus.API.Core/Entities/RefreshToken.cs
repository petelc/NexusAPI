using Traxs.SharedKernel;

namespace Nexus.API.Core.Entities;

/// <summary>
/// Refresh token entity for token rotation and security
/// Stored tokens can be invalidated and tracked
/// </summary>
public class RefreshToken : EntityBase<Guid>
{
  public Guid UserId { get; private set; }
  public string Token { get; private set; } = string.Empty;
  public string JwtId { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }
  public DateTime ExpiresAt { get; private set; }
  public bool Used { get; private set; }
  public DateTime? UsedAt { get; private set; }
  public bool Invalidated { get; private set; }
  public DateTime? InvalidatedAt { get; private set; }
  public string? InvalidatedReason { get; private set; }

  // For EF Core
  private RefreshToken() { }

  private RefreshToken(
    Guid id,
    Guid userId,
    string token,
    string jwtId,
    DateTime expiresAt)
  {
    Id = id;
    UserId = userId;
    Token = token;
    JwtId = jwtId;
    CreatedAt = DateTime.UtcNow;
    ExpiresAt = expiresAt;
    Used = false;
    Invalidated = false;
  }

  public static RefreshToken Create(
    Guid userId,
    string token,
    string jwtId,
    int daysValid = 7)
  {
    return new RefreshToken(
      Guid.NewGuid(),
      userId,
      token,
      jwtId,
      DateTime.UtcNow.AddDays(daysValid));
  }

  public void MarkAsUsed()
  {
    if (Used)
      throw new InvalidOperationException("Refresh token has already been used");
    
    if (Invalidated)
      throw new InvalidOperationException("Refresh token has been invalidated");
    
    if (IsExpired())
      throw new InvalidOperationException("Refresh token has expired");

    Used = true;
    UsedAt = DateTime.UtcNow;
  }

  public void Invalidate(string reason)
  {
    Invalidated = true;
    InvalidatedAt = DateTime.UtcNow;
    InvalidatedReason = reason;
  }

  public bool IsValid()
  {
    return !Used && !Invalidated && !IsExpired();
  }

  public bool IsExpired()
  {
    return DateTime.UtcNow > ExpiresAt;
  }
}
