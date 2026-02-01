using Nexus.API.Core.Models;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Interface for caching operations (Redis, MemoryCache, etc.)
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<bool> SetAsync<T>(
      string key,
      T value,
      TimeSpan? expiration = null,
      CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(
      string key,
      Func<Task<T>> factory,
      TimeSpan? expiration = null,
      CancellationToken cancellationToken = default);

    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    Task<bool> SetSlidingAsync<T>(
      string key,
      T value,
      TimeSpan slidingExpiration,
      CancellationToken cancellationToken = default);

    Task<long> IncrementAsync(
      string key,
      long value = 1,
      CancellationToken cancellationToken = default);

    Task<long> DecrementAsync(
      string key,
      long value = 1,
      CancellationToken cancellationToken = default);

    Task<bool> AddToSetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetSetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<bool> RemoveFromSetAsync<T>(
      string key,
      T value,
      CancellationToken cancellationToken = default);

    Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default);

    Task SubscribeAsync<T>(
      string channel,
      Action<T> handler,
      CancellationToken cancellationToken = default);

    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}