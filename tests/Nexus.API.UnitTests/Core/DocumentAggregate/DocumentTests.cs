using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.DocumentAggregate;

public class DocumentTests
{
  private readonly Guid _creatorId = Guid.NewGuid();

  private Document CreateDocument(
    string title = "Test Document",
    string content = "<p>Test content with several words</p>")
  {
    return Document.Create(
      Title.Create(title),
      DocumentContent.Create(content),
      _creatorId);
  }

  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public void Create_WithValidData_ReturnsDocument()
  {
    var doc = CreateDocument("My Document", "<p>Hello world</p>");

    doc.Title.Value.ShouldBe("My Document");
    doc.Content.RichText.ShouldBe("<p>Hello world</p>");
    doc.CreatedBy.ShouldBe(_creatorId);
    doc.Status.ShouldBe(DocumentStatus.Draft);
    doc.IsDeleted.ShouldBeFalse();
    doc.LanguageCode.ShouldBe("en-US");
  }

  [Fact]
  public void Create_SetsTimestamps()
  {
    var before = DateTime.UtcNow;
    var doc = CreateDocument();

    doc.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
    doc.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before);
  }

  [Fact]
  public void Create_CreatesInitialVersion()
  {
    var doc = CreateDocument();

    doc.Versions.Count.ShouldBe(1);
    doc.Versions.First().VersionNumber.ShouldBe(1);
    doc.Versions.First().CreatedBy.ShouldBe(_creatorId);
  }

  [Fact]
  public void Create_CalculatesReadingTime()
  {
    var doc = CreateDocument();

    doc.ReadingTimeMinutes.ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public void Create_WithCustomLanguageCode_SetsLanguageCode()
  {
    var doc = Document.Create(
      Title.Create("Doc"),
      DocumentContent.Create("<p>content</p>"),
      _creatorId,
      "fr-FR");

    doc.LanguageCode.ShouldBe("fr-FR");
  }

  [Fact]
  public void Create_WithEmptyCreatorId_ThrowsException()
  {
    Should.Throw<ArgumentException>(() =>
      Document.Create(
        Title.Create("Doc"),
        DocumentContent.Create("<p>content</p>"),
        Guid.Empty));
  }

  // ─── UpdateTitle ───────────────────────────────────────────────────

  [Fact]
  public void UpdateTitle_ChangesTitle()
  {
    var doc = CreateDocument();

    doc.UpdateTitle(Title.Create("New Title"), _creatorId);

    doc.Title.Value.ShouldBe("New Title");
  }

  [Fact]
  public void UpdateTitle_UpdatesTimestampAndLastEditedBy()
  {
    var doc = CreateDocument();
    var originalUpdatedAt = doc.UpdatedAt;

    doc.UpdateTitle(Title.Create("New Title"), _creatorId);

    doc.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
    doc.LastEditedBy.ShouldBe(_creatorId);
  }

  [Fact]
  public void UpdateTitle_WhenDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateTitle(Title.Create("New"), _creatorId));
  }

  [Fact]
  public void UpdateTitle_WhenArchived_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Archive(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateTitle(Title.Create("New"), _creatorId));
  }

  // ─── UpdateContent ─────────────────────────────────────────────────

  [Fact]
  public void UpdateContent_ChangesContent()
  {
    var doc = CreateDocument();
    var newContent = DocumentContent.Create("<p>Updated content</p>");

    doc.UpdateContent(newContent, _creatorId);

    doc.Content.RichText.ShouldBe("<p>Updated content</p>");
  }

  [Fact]
  public void UpdateContent_CreatesNewVersion()
  {
    var doc = CreateDocument();
    doc.Versions.Count.ShouldBe(1);

    doc.UpdateContent(DocumentContent.Create("<p>Updated</p>"), _creatorId);

    doc.Versions.Count.ShouldBe(2);
    doc.Versions.Last().VersionNumber.ShouldBe(2);
  }

  [Fact]
  public void UpdateContent_UpdatesReadingTime()
  {
    var doc = CreateDocument("Doc", "<p>Short</p>");
    var originalReadingTime = doc.ReadingTimeMinutes;

    var longContent = "<p>" + string.Join(" ", Enumerable.Repeat("word", 500)) + "</p>";
    doc.UpdateContent(DocumentContent.Create(longContent), _creatorId);

    doc.ReadingTimeMinutes.ShouldBeGreaterThanOrEqualTo(originalReadingTime);
  }

  [Fact]
  public void UpdateContent_WhenDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateContent(DocumentContent.Create("<p>new</p>"), _creatorId));
  }

  [Fact]
  public void UpdateContent_WhenArchived_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Archive(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateContent(DocumentContent.Create("<p>new</p>"), _creatorId));
  }

  // ─── Publish ───────────────────────────────────────────────────────

  [Fact]
  public void Publish_ChangesStatusToPublished()
  {
    var doc = CreateDocument();

    doc.Publish(_creatorId);

    doc.Status.ShouldBe(DocumentStatus.Published);
  }

  [Fact]
  public void Publish_WhenDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() => doc.Publish(_creatorId));
  }

  [Fact]
  public void Publish_WhenArchived_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Archive(_creatorId);

    Should.Throw<InvalidOperationException>(() => doc.Publish(_creatorId));
  }

  // ─── Archive ───────────────────────────────────────────────────────

  [Fact]
  public void Archive_ChangesStatusToArchived()
  {
    var doc = CreateDocument();

    doc.Archive(_creatorId);

    doc.Status.ShouldBe(DocumentStatus.Archived);
  }

  [Fact]
  public void Archive_PublishedDocument_ChangesStatusToArchived()
  {
    var doc = CreateDocument();
    doc.Publish(_creatorId);

    doc.Archive(_creatorId);

    doc.Status.ShouldBe(DocumentStatus.Archived);
  }

  [Fact]
  public void Archive_WhenDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() => doc.Archive(_creatorId));
  }

  // ─── Delete & Restore ──────────────────────────────────────────────

  [Fact]
  public void Delete_SoftDeletesDocument()
  {
    var doc = CreateDocument();

    doc.Delete(_creatorId);

    doc.IsDeleted.ShouldBeTrue();
    doc.DeletedAt.ShouldNotBeNull();
  }

  [Fact]
  public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() => doc.Delete(_creatorId));
  }

  [Fact]
  public void Restore_RestoresDeletedDocument()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    doc.Restore(_creatorId);

    doc.IsDeleted.ShouldBeFalse();
    doc.DeletedAt.ShouldBeNull();
  }

  [Fact]
  public void Restore_NotDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();

    Should.Throw<InvalidOperationException>(() => doc.Restore(_creatorId));
  }

  // ─── Tags ──────────────────────────────────────────────────────────

  [Fact]
  public void AddTag_AddsTagToDocument()
  {
    var doc = CreateDocument();
    var tag = Tag.Create("test-tag");

    doc.AddTag(tag);

    doc.Tags.Count.ShouldBe(1);
    doc.Tags.First().Name.ShouldBe("test-tag");
  }

  [Fact]
  public void AddTag_DuplicateTag_DoesNotAddTwice()
  {
    var doc = CreateDocument();
    var tag = Tag.Create("test-tag");

    doc.AddTag(tag);
    doc.AddTag(tag);

    doc.Tags.Count.ShouldBe(1);
  }

  [Fact]
  public void RemoveTag_RemovesTagFromDocument()
  {
    var doc = CreateDocument();
    var tag = Tag.Create("test-tag");
    doc.AddTag(tag);

    doc.RemoveTag(tag);

    doc.Tags.Count.ShouldBe(0);
  }

  [Fact]
  public void AddMultipleTags_AllAdded()
  {
    var doc = CreateDocument();
    doc.AddTag(Tag.Create("tag1"));
    doc.AddTag(Tag.Create("tag2"));
    doc.AddTag(Tag.Create("tag3"));

    doc.Tags.Count.ShouldBe(3);
  }

  // ─── CanEdit ───────────────────────────────────────────────────────

  [Fact]
  public void CanEdit_OwnerCanEdit()
  {
    var doc = CreateDocument();

    doc.CanEdit(UserId.Create(_creatorId)).ShouldBeTrue();
  }

  [Fact]
  public void CanEdit_NonOwnerCannotEdit()
  {
    var doc = CreateDocument();

    doc.CanEdit(UserId.CreateNew()).ShouldBeFalse();
  }

  [Fact]
  public void CanEdit_DeletedDocument_ReturnsFalse()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    doc.CanEdit(UserId.Create(_creatorId)).ShouldBeFalse();
  }

  [Fact]
  public void CanEdit_ArchivedDocument_ReturnsFalse()
  {
    var doc = CreateDocument();
    doc.Archive(_creatorId);

    doc.CanEdit(UserId.Create(_creatorId)).ShouldBeFalse();
  }

  // ─── UpdateLanguageCode ────────────────────────────────────────────

  [Fact]
  public void UpdateLanguageCode_ChangesLanguageCode()
  {
    var doc = CreateDocument();

    doc.UpdateLanguageCode("de-DE", _creatorId);

    doc.LanguageCode.ShouldBe("de-DE");
  }

  [Fact]
  public void UpdateLanguageCode_WhenDeleted_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Delete(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateLanguageCode("fr-FR", _creatorId));
  }

  [Fact]
  public void UpdateLanguageCode_WhenArchived_ThrowsInvalidOperationException()
  {
    var doc = CreateDocument();
    doc.Archive(_creatorId);

    Should.Throw<InvalidOperationException>(() =>
      doc.UpdateLanguageCode("fr-FR", _creatorId));
  }
}
