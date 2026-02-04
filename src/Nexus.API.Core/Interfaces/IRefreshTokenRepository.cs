using Nexus.API.Core.Entities;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for refresh token operations
/// Implements in Infrastructure layer
/// </summary>
public interface IRefreshTokenRepository
{
  Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
  Task<RefreshToken?> GetByJwtIdAsync(string jwtId, CancellationToken cancellationToken = default);
  Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
  Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
  Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
  Task InvalidateAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
  Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}
