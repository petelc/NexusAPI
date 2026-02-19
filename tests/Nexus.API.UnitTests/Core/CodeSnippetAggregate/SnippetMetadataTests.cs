using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.CodeSnippetAggregate;

public class SnippetMetadataTests
{
  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public void Create_SingleLineCode_LineCountIsOne()
  {
    var metadata = SnippetMetadata.Create("var x = 1;");

    metadata.LineCount.ShouldBe(1);
  }

  [Fact]
  public void Create_MultiLineCode_CorrectLineCount()
  {
    var code = "line1\nline2\nline3";
    var metadata = SnippetMetadata.Create(code);

    metadata.LineCount.ShouldBe(3);
  }

  [Fact]
  public void Create_CharacterCountMatchesCodeLength()
  {
    var code = "Hello World";
    var metadata = SnippetMetadata.Create(code);

    metadata.CharacterCount.ShouldBe(code.Length);
  }

  [Fact]
  public void Create_DefaultsToPrivate()
  {
    var metadata = SnippetMetadata.Create("code");

    metadata.IsPublic.ShouldBeFalse();
  }

  [Fact]
  public void Create_CanCreatePublic()
  {
    var metadata = SnippetMetadata.Create("code", isPublic: true);

    metadata.IsPublic.ShouldBeTrue();
  }

  [Fact]
  public void Create_InitializesCountsToZero()
  {
    var metadata = SnippetMetadata.Create("code");

    metadata.ForkCount.ShouldBe(0);
    metadata.ViewCount.ShouldBe(0);
  }

  // ─── UpdateFromCode ────────────────────────────────────────────────

  [Fact]
  public void UpdateFromCode_UpdatesLineAndCharacterCount()
  {
    var metadata = SnippetMetadata.Create("original");
    var newCode = "line1\nline2";

    var updated = metadata.UpdateFromCode(newCode);

    updated.LineCount.ShouldBe(2);
    updated.CharacterCount.ShouldBe(newCode.Length);
  }

  [Fact]
  public void UpdateFromCode_PreservesVisibility()
  {
    var metadata = SnippetMetadata.Create("code", isPublic: true);

    var updated = metadata.UpdateFromCode("new code");

    updated.IsPublic.ShouldBeTrue();
  }

  [Fact]
  public void UpdateFromCode_PreservesForkAndViewCounts()
  {
    var metadata = SnippetMetadata.Create("code")
      .IncrementForkCount()
      .IncrementViewCount()
      .IncrementViewCount();

    var updated = metadata.UpdateFromCode("new code");

    updated.ForkCount.ShouldBe(1);
    updated.ViewCount.ShouldBe(2);
  }

  // ─── Visibility ────────────────────────────────────────────────────

  [Fact]
  public void MakePublic_SetsIsPublicTrue()
  {
    var metadata = SnippetMetadata.Create("code");

    var updated = metadata.MakePublic();

    updated.IsPublic.ShouldBeTrue();
  }

  [Fact]
  public void MakePrivate_SetsIsPublicFalse()
  {
    var metadata = SnippetMetadata.Create("code", isPublic: true);

    var updated = metadata.MakePrivate();

    updated.IsPublic.ShouldBeFalse();
  }

  [Fact]
  public void MakePublic_PreservesOtherFields()
  {
    var metadata = SnippetMetadata.Create("line1\nline2")
      .IncrementForkCount();

    var updated = metadata.MakePublic();

    updated.LineCount.ShouldBe(metadata.LineCount);
    updated.ForkCount.ShouldBe(1);
  }

  // ─── Counters ──────────────────────────────────────────────────────

  [Fact]
  public void IncrementForkCount_IncrementsByOne()
  {
    var metadata = SnippetMetadata.Create("code");

    var updated = metadata.IncrementForkCount();

    updated.ForkCount.ShouldBe(1);
  }

  [Fact]
  public void IncrementForkCount_Accumulates()
  {
    var metadata = SnippetMetadata.Create("code")
      .IncrementForkCount()
      .IncrementForkCount()
      .IncrementForkCount();

    metadata.ForkCount.ShouldBe(3);
  }

  [Fact]
  public void IncrementViewCount_IncrementsByOne()
  {
    var metadata = SnippetMetadata.Create("code");

    var updated = metadata.IncrementViewCount();

    updated.ViewCount.ShouldBe(1);
  }

  [Fact]
  public void IncrementViewCount_Accumulates()
  {
    var metadata = SnippetMetadata.Create("code")
      .IncrementViewCount()
      .IncrementViewCount();

    metadata.ViewCount.ShouldBe(2);
  }

  // ─── Immutability ──────────────────────────────────────────────────

  [Fact]
  public void MakePublic_ReturnsNewInstance()
  {
    var original = SnippetMetadata.Create("code");
    var updated = original.MakePublic();

    original.IsPublic.ShouldBeFalse();
    updated.IsPublic.ShouldBeTrue();
  }

  [Fact]
  public void IncrementViewCount_ReturnsNewInstance()
  {
    var original = SnippetMetadata.Create("code");
    var updated = original.IncrementViewCount();

    original.ViewCount.ShouldBe(0);
    updated.ViewCount.ShouldBe(1);
  }
}
