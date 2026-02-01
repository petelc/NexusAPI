namespace Nexus.API.Core.Models;

/// <summary>
/// Search results container
/// </summary>
public class SearchResults
{
  public List<SearchResult> Results { get; set; } = new();
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}

/// <summary>
/// Individual search result
/// </summary>
public class SearchResult
{
  public Guid DocumentId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Excerpt { get; set; } = string.Empty;
  public double Score { get; set; }
  public List<string> Tags { get; set; } = new();
  public DateTime IndexedAt { get; set; }
}
