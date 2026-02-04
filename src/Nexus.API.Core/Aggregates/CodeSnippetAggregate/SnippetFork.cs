namespace Nexus.API.Core.Aggregates.CodeSnippetAggregate;

/// <summary>
/// Entity representing a fork relationship between snippets
/// Tracks when and who forked a snippet
/// </summary>
public class SnippetFork
{
  public Guid OriginalSnippetId { get; private set; }
  public Guid ForkedSnippetId { get; private set; }
  public Guid ForkedBy { get; private set; }
  public DateTime ForkedAt { get; private set; }

  // For EF Core
  private SnippetFork() { }

  public SnippetFork(
    Guid originalSnippetId,
    Guid forkedSnippetId,
    Guid forkedBy,
    DateTime forkedAt)
  {
    OriginalSnippetId = originalSnippetId;
    ForkedSnippetId = forkedSnippetId;
    ForkedBy = forkedBy;
    ForkedAt = forkedAt;
  }
}
