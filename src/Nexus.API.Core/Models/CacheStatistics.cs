namespace Nexus.API.Core.Models;

/// <summary>
/// Cache statistics model
/// </summary>
public class CacheStatistics
{
  public long TotalKeys { get; set; }
  public long Hits { get; set; }
  public long Misses { get; set; }
  public double HitRate { get; set; }
}
