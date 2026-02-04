using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Entities;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for RefreshToken entity
/// Handles token storage and retrieval with security features
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
  private readonly AppDbContext _dbContext;

  public RefreshTokenRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<RefreshToken?> GetByTokenAsync(
    string token, 
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<RefreshToken>()
      .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
  }

  public async Task<RefreshToken?> GetByJwtIdAsync(
    string jwtId, 
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<RefreshToken>()
      .FirstOrDefaultAsync(rt => rt.JwtId == jwtId, cancellationToken);
  }

  public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(
    Guid userId, 
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<RefreshToken>()
      .Where(rt => rt.UserId == userId 
        && !rt.Used 
        && !rt.Invalidated 
        && rt.ExpiresAt > DateTime.UtcNow)
      .ToListAsync(cancellationToken);
  }

  public async Task AddAsync(
    RefreshToken refreshToken, 
    CancellationToken cancellationToken = default)
  {
    await _dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(
    RefreshToken refreshToken, 
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<RefreshToken>().Update(refreshToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task InvalidateAllUserTokensAsync(
    Guid userId, 
    string reason, 
    CancellationToken cancellationToken = default)
  {
    var tokens = await _dbContext.Set<RefreshToken>()
      .Where(rt => rt.UserId == userId && !rt.Invalidated)
      .ToListAsync(cancellationToken);

    foreach (var token in tokens)
    {
      token.Invalidate(reason);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
  {
    var expiredTokens = await _dbContext.Set<RefreshToken>()
      .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
      .ToListAsync(cancellationToken);

    _dbContext.Set<RefreshToken>().RemoveRange(expiredTokens);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
