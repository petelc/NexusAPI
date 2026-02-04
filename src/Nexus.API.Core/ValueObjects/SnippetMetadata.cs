using Traxs.SharedKernel;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Value object containing code snippet metadata
/// Tracks statistics and visibility
/// </summary>
public class SnippetMetadata : ValueObject
{
  public int LineCount { get; private set; }
  public int CharacterCount { get; private set; }
  public bool IsPublic { get; private set; }
  public int ForkCount { get; private set; }
  public int ViewCount { get; private set; }

  private SnippetMetadata(
    int lineCount,
    int characterCount,
    bool isPublic,
    int forkCount = 0,
    int viewCount = 0)
  {
    LineCount = lineCount;
    CharacterCount = characterCount;
    IsPublic = isPublic;
    ForkCount = forkCount;
    ViewCount = viewCount;
  }

  public static SnippetMetadata Create(string code, bool isPublic = false)
  {
    var lineCount = CountLines(code);
    var characterCount = code.Length;

    return new SnippetMetadata(lineCount, characterCount, isPublic);
  }

  public SnippetMetadata UpdateFromCode(string code)
  {
    var lineCount = CountLines(code);
    var characterCount = code.Length;

    return new SnippetMetadata(lineCount, characterCount, IsPublic, ForkCount, ViewCount);
  }

  public SnippetMetadata MakePublic()
  {
    return new SnippetMetadata(LineCount, CharacterCount, true, ForkCount, ViewCount);
  }

  public SnippetMetadata MakePrivate()
  {
    return new SnippetMetadata(LineCount, CharacterCount, false, ForkCount, ViewCount);
  }

  public SnippetMetadata IncrementForkCount()
  {
    return new SnippetMetadata(LineCount, CharacterCount, IsPublic, ForkCount + 1, ViewCount);
  }

  public SnippetMetadata IncrementViewCount()
  {
    return new SnippetMetadata(LineCount, CharacterCount, IsPublic, ForkCount, ViewCount + 1);
  }

  private static int CountLines(string code)
  {
    if (string.IsNullOrEmpty(code))
      return 0;

    var lineCount = 1;
    foreach (var c in code)
    {
      if (c == '\n')
        lineCount++;
    }
    return lineCount;
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return LineCount;
    yield return CharacterCount;
    yield return IsPublic;
    yield return ForkCount;
    yield return ViewCount;
  }
}
