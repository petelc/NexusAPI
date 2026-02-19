using Ardalis.GuardClauses;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate.Events;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.CodeSnippetAggregate;

/// <summary>
/// CodeSnippet aggregate root
/// Represents a code snippet with language-specific metadata and syntax highlighting
/// </summary>
public class CodeSnippet : EntityBase<Guid>, IAggregateRoot
{
  private readonly List<Tag> _tags = new();
  private readonly List<SnippetFork> _forks = new();

  // Properties
  public Title Title { get; private set; } = null!;
  public string Code { get; private set; } = string.Empty;
  public ProgrammingLanguage Language { get; private set; } = null!;
  public string? Description { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public SnippetMetadata Metadata { get; private set; } = null!;
  public Guid? OriginalSnippetId { get; private set; }
  public bool IsDeleted { get; private set; }
  public DateTime? DeletedAt { get; private set; }

  // Collections
  public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
  public IReadOnlyCollection<SnippetFork> Forks => _forks.AsReadOnly();

  // Parameterless constructor for EF Core
  private CodeSnippet() { }

  /// <summary>
  /// Factory method to create a new code snippet
  /// </summary>
  public static CodeSnippet Create(
    Title title,
    string code,
    ProgrammingLanguage language,
    Guid createdBy)
  {
    Guard.Against.Null(title, nameof(title));
    Guard.Against.NullOrWhiteSpace(code, nameof(code));
    Guard.Against.Null(language, nameof(language));

    var snippet = new CodeSnippet
    {
      Id = Guid.NewGuid(),
      Title = title,
      Code = code,
      Language = language,
      CreatedBy = createdBy,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      Metadata = SnippetMetadata.Create(code, isPublic: false),
      IsDeleted = false
    };

    snippet.RegisterDomainEvent(new SnippetCreatedEvent(snippet.Id, createdBy));
    return snippet;
  }

  /// <summary>
  /// Update snippet properties
  /// Only updates properties that are provided (not null)
  /// </summary>
  public void Update(Title? title = null, string? code = null, string? description = null)
  {
    var hasChanges = false;

    if (title is not null)
    {
      if (Title.Value != title.Value)
      {
        Title = title;
        hasChanges = true;
      }
    }

    if (code != null && Code != code)
    {
      Guard.Against.NullOrWhiteSpace(code, nameof(code));
      Code = code;
      Metadata = Metadata.UpdateFromCode(code);
      hasChanges = true;
    }

    if (description != null && Description != description)
    {
      Description = description;
      hasChanges = true;
    }

    if (hasChanges)
    {
      UpdatedAt = DateTime.UtcNow;
      RegisterDomainEvent(new SnippetUpdatedEvent(Id));
    }
  }

  /// <summary>
  /// Make the snippet publicly accessible
  /// </summary>
  public void MakePublic()
  {
    if (!Metadata.IsPublic)
    {
      Metadata = Metadata.MakePublic();
      UpdatedAt = DateTime.UtcNow;
      RegisterDomainEvent(new SnippetMadePublicEvent(Id));
    }
  }

  /// <summary>
  /// Make the snippet private
  /// Cannot be made private if it has been forked
  /// </summary>
  public void MakePrivate()
  {
    if (_forks.Any())
    {
      throw new DomainException("Cannot make snippet private after it has been forked");
    }

    if (Metadata.IsPublic)
    {
      Metadata = Metadata.MakePrivate();
      UpdatedAt = DateTime.UtcNow;
      RegisterDomainEvent(new SnippetMadePrivateEvent(Id));
    }
  }

  /// <summary>
  /// Create a fork (copy) of this snippet
  /// Only public snippets can be forked
  /// </summary>
  public CodeSnippet Fork(Guid userId, Title newTitle)
  {
    if (!Metadata.IsPublic)
    {
      throw new DomainException("Cannot fork a private snippet");
    }

    // Create the forked snippet with a fresh ProgrammingLanguage instance.
    // Sharing the same Language reference would cause EF Core to try to reassign
    // the shadow CodeSnippetId key when the fork is tracked, throwing an exception.
    var forkLanguage = ProgrammingLanguage.Create(Language.Name, Language.FileExtension, Language.Version);
    var fork = Create(newTitle, Code, forkLanguage, userId);
    fork.OriginalSnippetId = Id;

    if (!string.IsNullOrEmpty(Description))
    {
      fork.Description = Description;
    }

    // Track the fork relationship
    var forkReference = new SnippetFork(Id, fork.Id, userId, DateTime.UtcNow);
    _forks.Add(forkReference);

    // Increment fork count
    Metadata = Metadata.IncrementForkCount();
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new SnippetForkedEvent(Id, fork.Id, userId));

    return fork;
  }

  /// <summary>
  /// Increment the view count
  /// Should be called when a snippet is viewed by a non-owner
  /// </summary>
  public void IncrementViewCount()
  {
    Metadata = Metadata.IncrementViewCount();
    // Don't update UpdatedAt for view count changes
  }

  /// <summary>
  /// Add a tag to the snippet
  /// </summary>
  public void AddTag(Tag tag)
  {
    Guard.Against.Null(tag, nameof(tag));

    if (!_tags.Any(t => t.Id == tag.Id))
    {
      _tags.Add(tag);
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Remove a tag from the snippet
  /// </summary>
  public void RemoveTag(Tag tag)
  {
    Guard.Against.Null(tag, nameof(tag));

    if (_tags.Remove(tag))
    {
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Clear all tags from the snippet
  /// </summary>
  public void ClearTags()
  {
    if (_tags.Any())
    {
      _tags.Clear();
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Soft delete the snippet
  /// </summary>
  public void Delete()
  {
    if (!IsDeleted)
    {
      IsDeleted = true;
      DeletedAt = DateTime.UtcNow;
      RegisterDomainEvent(new SnippetDeletedEvent(Id, CreatedBy));
    }
  }

  /// <summary>
  /// Check if a user can edit this snippet
  /// </summary>
  public bool CanEdit(Guid userId)
  {
    return CreatedBy == userId;
  }

  /// <summary>
  /// Check if a user can view this snippet
  /// </summary>
  public bool CanView(Guid userId)
  {
    return Metadata.IsPublic || CreatedBy == userId;
  }
}
