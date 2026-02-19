using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.CodeSnippetAggregate;

public class CodeSnippetTests
{
  private readonly Guid _ownerId = Guid.NewGuid();
  private readonly Guid _otherId = Guid.NewGuid();

  private CodeSnippet CreateSnippet(
    string title = "My Snippet",
    string code = "Console.WriteLine(\"Hello\");",
    string language = "C#")
  {
    return CodeSnippet.Create(
      Title.Create(title),
      code,
      ProgrammingLanguage.Create(language, "cs"),
      _ownerId);
  }

  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public void Create_WithValidData_ReturnsSnippet()
  {
    var snippet = CreateSnippet("My Snippet", "var x = 1;");

    snippet.Title.Value.ShouldBe("My Snippet");
    snippet.Code.ShouldBe("var x = 1;");
    snippet.Language.Name.ShouldBe("C#");
    snippet.CreatedBy.ShouldBe(_ownerId);
    snippet.IsDeleted.ShouldBeFalse();
    snippet.Metadata.IsPublic.ShouldBeFalse();
    snippet.Id.ShouldNotBe(Guid.Empty);
  }

  [Fact]
  public void Create_SetsTimestamps()
  {
    var before = DateTime.UtcNow;
    var snippet = CreateSnippet();

    snippet.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
    snippet.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before);
  }

  [Fact]
  public void Create_WithNullTitle_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      CodeSnippet.Create(null!, "var x = 1;", ProgrammingLanguage.CSharp(), _ownerId));
  }

  [Fact]
  public void Create_WithEmptyCode_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      CodeSnippet.Create(Title.Create("Title"), "", ProgrammingLanguage.CSharp(), _ownerId));
  }

  [Fact]
  public void Create_WithNullLanguage_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      CodeSnippet.Create(Title.Create("Title"), "code", null!, _ownerId));
  }

  [Fact]
  public void Create_InitializesMetadata()
  {
    var code = "line1\nline2\nline3";
    var snippet = CodeSnippet.Create(
      Title.Create("Title"), code, ProgrammingLanguage.CSharp(), _ownerId);

    snippet.Metadata.LineCount.ShouldBe(3);
    snippet.Metadata.CharacterCount.ShouldBe(code.Length);
    snippet.Metadata.ForkCount.ShouldBe(0);
    snippet.Metadata.ViewCount.ShouldBe(0);
  }

  [Fact]
  public void Create_NoForksNoTags()
  {
    var snippet = CreateSnippet();

    snippet.Forks.ShouldBeEmpty();
    snippet.Tags.ShouldBeEmpty();
    snippet.OriginalSnippetId.ShouldBeNull();
  }

  // ─── Update ────────────────────────────────────────────────────────

  [Fact]
  public void Update_WithNewTitle_UpdatesTitle()
  {
    var snippet = CreateSnippet("Old Title");
    var newTitle = Title.Create("New Title");

    snippet.Update(title: newTitle);

    snippet.Title.Value.ShouldBe("New Title");
  }

  [Fact]
  public void Update_WithSameTitle_NoTimestampChange()
  {
    var snippet = CreateSnippet("Same Title");
    var before = snippet.UpdatedAt;

    snippet.Update(title: Title.Create("Same Title"));

    snippet.UpdatedAt.ShouldBe(before);
  }

  [Fact]
  public void Update_WithNewCode_UpdatesCodeAndMetadata()
  {
    var snippet = CreateSnippet(code: "old code");
    var newCode = "new code\nline 2";

    snippet.Update(code: newCode);

    snippet.Code.ShouldBe(newCode);
    snippet.Metadata.LineCount.ShouldBe(2);
  }

  [Fact]
  public void Update_WithNewDescription_UpdatesDescription()
  {
    var snippet = CreateSnippet();

    snippet.Update(description: "Updated description");

    snippet.Description.ShouldBe("Updated description");
  }

  [Fact]
  public void Update_WithChanges_UpdatesTimestamp()
  {
    var snippet = CreateSnippet("Original");
    var before = snippet.UpdatedAt;

    snippet.Update(title: Title.Create("Changed"));

    snippet.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before);
  }

  // ─── Visibility ────────────────────────────────────────────────────

  [Fact]
  public void MakePublic_WhenPrivate_SetsIsPublicTrue()
  {
    var snippet = CreateSnippet();

    snippet.MakePublic();

    snippet.Metadata.IsPublic.ShouldBeTrue();
  }

  [Fact]
  public void MakePublic_WhenAlreadyPublic_NoChange()
  {
    var snippet = CreateSnippet();
    snippet.MakePublic();
    var beforeUpdate = snippet.UpdatedAt;

    snippet.MakePublic();

    snippet.Metadata.IsPublic.ShouldBeTrue();
    snippet.UpdatedAt.ShouldBe(beforeUpdate);
  }

  [Fact]
  public void MakePrivate_WhenPublicAndNotForked_SetsIsPublicFalse()
  {
    var snippet = CreateSnippet();
    snippet.MakePublic();

    snippet.MakePrivate();

    snippet.Metadata.IsPublic.ShouldBeFalse();
  }

  [Fact]
  public void MakePrivate_WhenAlreadyPrivate_NoChange()
  {
    var snippet = CreateSnippet();
    var beforeUpdate = snippet.UpdatedAt;

    snippet.MakePrivate();

    snippet.Metadata.IsPublic.ShouldBeFalse();
    snippet.UpdatedAt.ShouldBe(beforeUpdate);
  }

  [Fact]
  public void MakePrivate_WhenForked_ThrowsDomainException()
  {
    var snippet = CreateSnippet();
    snippet.MakePublic();
    snippet.Fork(_otherId, Title.Create("Fork Title"));

    Should.Throw<DomainException>(() => snippet.MakePrivate());
  }

  // ─── Fork ──────────────────────────────────────────────────────────

  [Fact]
  public void Fork_PublicSnippet_ReturnsForkedSnippet()
  {
    var original = CreateSnippet("Original");
    original.MakePublic();

    var fork = original.Fork(_otherId, Title.Create("My Fork"));

    fork.ShouldNotBeNull();
    fork.Title.Value.ShouldBe("My Fork");
    fork.Code.ShouldBe(original.Code);
    fork.Language.Name.ShouldBe(original.Language.Name);
    fork.CreatedBy.ShouldBe(_otherId);
    fork.OriginalSnippetId.ShouldBe(original.Id);
  }

  [Fact]
  public void Fork_PublicSnippet_IncrementsForkCount()
  {
    var original = CreateSnippet();
    original.MakePublic();

    original.Fork(_otherId, Title.Create("Fork"));

    original.Metadata.ForkCount.ShouldBe(1);
  }

  [Fact]
  public void Fork_PublicSnippet_TracksForkRelationship()
  {
    var original = CreateSnippet();
    original.MakePublic();
    var fork = original.Fork(_otherId, Title.Create("Fork"));

    original.Forks.ShouldHaveSingleItem();
    original.Forks.First().ForkedSnippetId.ShouldBe(fork.Id);
    original.Forks.First().ForkedBy.ShouldBe(_otherId);
  }

  [Fact]
  public void Fork_PrivateSnippet_ThrowsDomainException()
  {
    var original = CreateSnippet();

    Should.Throw<DomainException>(() =>
      original.Fork(_otherId, Title.Create("Fork")));
  }

  // ─── ViewCount ─────────────────────────────────────────────────────

  [Fact]
  public void IncrementViewCount_IncrementsByOne()
  {
    var snippet = CreateSnippet();

    snippet.IncrementViewCount();
    snippet.IncrementViewCount();

    snippet.Metadata.ViewCount.ShouldBe(2);
  }

  [Fact]
  public void IncrementViewCount_DoesNotUpdateTimestamp()
  {
    var snippet = CreateSnippet();
    var before = snippet.UpdatedAt;

    snippet.IncrementViewCount();

    snippet.UpdatedAt.ShouldBe(before);
  }

  // ─── Tags ──────────────────────────────────────────────────────────

  [Fact]
  public void AddTag_AddsTagToCollection()
  {
    var snippet = CreateSnippet();
    var tag = Nexus.API.Core.Aggregates.DocumentAggregate.Tag.Create("csharp");

    snippet.AddTag(tag);

    snippet.Tags.ShouldHaveSingleItem();
    snippet.Tags.First().Name.ShouldBe("csharp");
  }

  [Fact]
  public void AddTag_DuplicateById_NotAdded()
  {
    var snippet = CreateSnippet();
    var tag = Nexus.API.Core.Aggregates.DocumentAggregate.Tag.Create("csharp");

    snippet.AddTag(tag);
    snippet.AddTag(tag);

    snippet.Tags.Count.ShouldBe(1);
  }

  [Fact]
  public void AddTag_NullTag_ThrowsException()
  {
    var snippet = CreateSnippet();

    Should.Throw<Exception>(() => snippet.AddTag(null!));
  }

  [Fact]
  public void RemoveTag_ExistingTag_RemovesFromCollection()
  {
    var snippet = CreateSnippet();
    var tag = Nexus.API.Core.Aggregates.DocumentAggregate.Tag.Create("csharp");
    snippet.AddTag(tag);

    snippet.RemoveTag(tag);

    snippet.Tags.ShouldBeEmpty();
  }

  [Fact]
  public void ClearTags_RemovesAllTags()
  {
    var snippet = CreateSnippet();
    snippet.AddTag(Nexus.API.Core.Aggregates.DocumentAggregate.Tag.Create("tag1"));
    snippet.AddTag(Nexus.API.Core.Aggregates.DocumentAggregate.Tag.Create("tag2"));

    snippet.ClearTags();

    snippet.Tags.ShouldBeEmpty();
  }

  [Fact]
  public void ClearTags_WhenEmpty_NoChange()
  {
    var snippet = CreateSnippet();
    var before = snippet.UpdatedAt;

    snippet.ClearTags();

    snippet.UpdatedAt.ShouldBe(before);
  }

  // ─── Delete ────────────────────────────────────────────────────────

  [Fact]
  public void Delete_SetsIsDeletedTrue()
  {
    var snippet = CreateSnippet();

    snippet.Delete();

    snippet.IsDeleted.ShouldBeTrue();
    snippet.DeletedAt.ShouldNotBeNull();
  }

  [Fact]
  public void Delete_WhenAlreadyDeleted_NoChange()
  {
    var snippet = CreateSnippet();
    snippet.Delete();
    var deletedAt = snippet.DeletedAt;

    snippet.Delete();

    snippet.DeletedAt.ShouldBe(deletedAt);
  }

  // ─── Permissions ───────────────────────────────────────────────────

  [Fact]
  public void CanEdit_ByOwner_ReturnsTrue()
  {
    var snippet = CreateSnippet();

    snippet.CanEdit(_ownerId).ShouldBeTrue();
  }

  [Fact]
  public void CanEdit_ByOtherUser_ReturnsFalse()
  {
    var snippet = CreateSnippet();

    snippet.CanEdit(_otherId).ShouldBeFalse();
  }

  [Fact]
  public void CanView_PublicSnippet_ByAnyUser_ReturnsTrue()
  {
    var snippet = CreateSnippet();
    snippet.MakePublic();

    snippet.CanView(_otherId).ShouldBeTrue();
  }

  [Fact]
  public void CanView_PrivateSnippet_ByOwner_ReturnsTrue()
  {
    var snippet = CreateSnippet();

    snippet.CanView(_ownerId).ShouldBeTrue();
  }

  [Fact]
  public void CanView_PrivateSnippet_ByOther_ReturnsFalse()
  {
    var snippet = CreateSnippet();

    snippet.CanView(_otherId).ShouldBeFalse();
  }
}
