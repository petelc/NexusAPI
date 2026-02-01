using StackExchange.Redis;
using System.Text.Json;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.Models;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Redis caching service for distributed caching and performance optimization.
/// Implements the ICacheService interface from the Core layer.
/// </summary>
public class RedisCacheService : ICacheService
{
  private readonly IConnectionMultiplexer _redis;
  private readonly IDatabase _database;
  private readonly ILogger<RedisCacheService> _logger;
  private readonly JsonSerializerOptions _jsonOptions;

  public RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger)
  {
    _redis = redis;
    _database = redis.GetDatabase();
    _logger = logger;
    _jsonOptions = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
      WriteIndented = false
    };
  }

  /// <summary>
  /// Get a value from cache
  /// </summary>
  public async Task<T?> GetAsync<T>(
    string key,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var value = await _database.StringGetAsync(key);

      if (value.IsNullOrEmpty)
      {
        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
      }

      _logger.LogDebug("Cache hit for key: {Key}", key);
      return JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
      return default;
    }
  }

  /// <summary>
  /// Set a value in cache with expiration
  /// </summary>
  public async Task<bool> SetAsync<T>(
    string key,
    T value,
    TimeSpan? expiration = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var serialized = JsonSerializer.Serialize(value, _jsonOptions);

      var success = await _database.StringSetAsync(
        key,
        serialized,
        expiration);

      if (success)
      {
        _logger.LogDebug("Value set in cache for key: {Key}, expiration: {Expiration}",
          key, expiration?.ToString() ?? "never");
      }

      return success;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Remove a value from cache
  /// </summary>
  public async Task<bool> RemoveAsync(
    string key,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var success = await _database.KeyDeleteAsync(key);

      if (success)
      {
        _logger.LogDebug("Key removed from cache: {Key}", key);
      }

      return success;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing key from cache: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Check if a key exists in cache
  /// </summary>
  public async Task<bool> ExistsAsync(
    string key,
    CancellationToken cancellationToken = default)
  {
    try
    {
      return await _database.KeyExistsAsync(key);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Get or create a cached value
  /// </summary>
  public async Task<T> GetOrCreateAsync<T>(
    string key,
    Func<Task<T>> factory,
    TimeSpan? expiration = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      // Try to get from cache first
      var cached = await GetAsync<T>(key, cancellationToken);
      if (cached != null)
      {
        return cached;
      }

      // Cache miss - execute factory
      var value = await factory();

      // Store in cache
      await SetAsync(key, value, expiration, cancellationToken);

      return value;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
      throw;
    }
  }

  /// <summary>
  /// Invalidate cache by pattern (use carefully!)
  /// </summary>
  public async Task InvalidateByPatternAsync(
    string pattern,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var endpoints = _redis.GetEndPoints();
      var server = _redis.GetServer(endpoints.First());

      var keys = server.Keys(pattern: pattern).ToArray();

      if (keys.Length > 0)
      {
        await _database.KeyDeleteAsync(keys);
        _logger.LogInformation("Invalidated {Count} keys matching pattern: {Pattern}",
          keys.Length, pattern);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
    }
  }

  /// <summary>
  /// Set cache with sliding expiration
  /// </summary>
  public async Task<bool> SetSlidingAsync<T>(
    string key,
    T value,
    TimeSpan slidingExpiration,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var serialized = JsonSerializer.Serialize(value, _jsonOptions);

      // Store value
      var success = await _database.StringSetAsync(key, serialized, slidingExpiration);

      if (success)
      {
        // Store metadata for sliding expiration
        await _database.HashSetAsync(
          $"{key}:metadata",
          new[]
          {
            new HashEntry("slidingExpiration", slidingExpiration.TotalSeconds),
            new HashEntry("lastAccess", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
          });

        _logger.LogDebug("Value set in cache with sliding expiration for key: {Key}", key);
      }

      return success;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting sliding cache for key: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Increment a counter in cache
  /// </summary>
  public async Task<long> IncrementAsync(
    string key,
    long value = 1,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await _database.StringIncrementAsync(key, value);
      _logger.LogDebug("Incremented key {Key} by {Value}, new value: {Result}",
        key, value, result);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error incrementing key: {Key}", key);
      return 0;
    }
  }

  /// <summary>
  /// Decrement a counter in cache
  /// </summary>
  public async Task<long> DecrementAsync(
    string key,
    long value = 1,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await _database.StringDecrementAsync(key, value);
      _logger.LogDebug("Decremented key {Key} by {Value}, new value: {Result}",
        key, value, result);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error decrementing key: {Key}", key);
      return 0;
    }
  }

  /// <summary>
  /// Add item to a set
  /// </summary>
  public async Task<bool> AddToSetAsync<T>(
    string key,
    T value,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var serialized = JsonSerializer.Serialize(value, _jsonOptions);
      var success = await _database.SetAddAsync(key, serialized);

      if (success)
      {
        _logger.LogDebug("Added value to set: {Key}", key);
      }

      return success;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error adding to set: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Get all items from a set
  /// </summary>
  public async Task<IEnumerable<T>> GetSetAsync<T>(
    string key,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var values = await _database.SetMembersAsync(key);

      return values
        .Select(v => JsonSerializer.Deserialize<T>(v.ToString(), _jsonOptions))
        .Where(v => v != null)
        .Cast<T>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting set: {Key}", key);
      return Enumerable.Empty<T>();
    }
  }

  /// <summary>
  /// Remove item from set
  /// </summary>
  public async Task<bool> RemoveFromSetAsync<T>(
    string key,
    T value,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var serialized = JsonSerializer.Serialize(value, _jsonOptions);
      return await _database.SetRemoveAsync(key, serialized);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing from set: {Key}", key);
      return false;
    }
  }

  /// <summary>
  /// Publish a message to a Redis channel
  /// </summary>
  public async Task PublishAsync<T>(
    string channel,
    T message,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var subscriber = _redis.GetSubscriber();
      var serialized = JsonSerializer.Serialize(message, _jsonOptions);

      await subscriber.PublishAsync(RedisChannel.Literal(channel), serialized);

      _logger.LogDebug("Published message to channel: {Channel}", channel);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing to channel: {Channel}", channel);
    }
  }

  /// <summary>
  /// Subscribe to a Redis channel
  /// </summary>
  public async Task SubscribeAsync<T>(
    string channel,
    Action<T> handler,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var subscriber = _redis.GetSubscriber();

      await subscriber.SubscribeAsync(
        RedisChannel.Literal(channel),
        (ch, message) =>
        {
          try
          {
            var deserialized = JsonSerializer.Deserialize<T>(message.ToString()!, _jsonOptions);
            if (deserialized != null)
            {
              handler(deserialized);
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error handling message from channel: {Channel}", channel);
          }
        });

      _logger.LogInformation("Subscribed to channel: {Channel}", channel);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error subscribing to channel: {Channel}", channel);
    }
  }

  /// <summary>
  /// Get cache statistics
  /// </summary>
  public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var server = _redis.GetServer(_redis.GetEndPoints().First());
      var info = await server.InfoAsync("stats");

      var statsDict = info.SelectMany(g => g).ToDictionary(x => x.Key, x => x.Value);

      var stats = new CacheStatistics
      {
        TotalKeys = (await server.DatabaseSizeAsync()),
        Hits = statsDict.ContainsKey("keyspace_hits") ? long.Parse(statsDict["keyspace_hits"]) : 0,
        Misses = statsDict.ContainsKey("keyspace_misses") ? long.Parse(statsDict["keyspace_misses"]) : 0
      };

      stats.HitRate = stats.Hits + stats.Misses > 0
        ? (double)stats.Hits / (stats.Hits + stats.Misses) * 100
        : 0;

      return stats;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting cache statistics");
      return new CacheStatistics();
    }
  }
}
