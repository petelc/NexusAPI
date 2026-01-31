using Ardalis.GuardClauses;
using Nexus.Core.Enums;
using Nexus.Core.Events;
using Nexus.Core.ValueObjects;
using Traxs.SharedKernel;
using Traxs.SharedKernel.Interfaces;

namespace Nexus.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Document aggregate root - represents a rich-text document with versioning
/// </summary>
public class Document : EntityBase<DocumentId>, IAggregateRoot
{
    private readonly List<DocumentVersion> _versions = new();
    private readonly List<Tag> _tags = new();
    
    public Title Title { get; private set; } = null!;
    public DocumentContent Content { get; private set; } = null!;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? LastEditedBy { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int ReadingTimeMinutes { get; private set; }
    public string LanguageCode { get; private set; } = "en-US";
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Navigation properties
    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    
    // Private constructor for EF Core
    private Document() { }
    
    /// <summary>
    /// Factory method to create a new document
    /// </summary>
    public static Document Create(Title title, DocumentContent content, Guid createdBy, string? languageCode = null)
    {
        Guard.Against.Null(title, nameof(title));
        Guard.Against.Null(content, nameof(content));
        Guard.Against.Default(createdBy, nameof(createdBy));
        
        var document = new Document
        {
            Id = DocumentId.CreateNew(),
            Title = title,
            Content = content,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = DocumentStatus.Draft,
            ReadingTimeMinutes = CalculateReadingTime(content.WordCount),
            LanguageCode = languageCode ?? "en-US",
            IsDeleted = false
        };
        
        // Raise domain event
        document.RegisterDomainEvent(new DocumentCreatedEvent(document.Id, createdBy));
        
        return document;
    }
    
    /// <summary>
    /// Update the document content and create a new version
    /// </summary>
    public void UpdateContent(DocumentContent newContent, Guid userId)
    {
        Guard.Against.Null(newContent, nameof(newContent));
        Guard.Against.Default(userId, nameof(userId));
        
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted document");
            
        if (Status == DocumentStatus.Archived)
            throw new InvalidOperationException("Cannot update an archived document");
        
        // Create version before updating
        CreateVersion();
        
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        LastEditedBy = userId;
        ReadingTimeMinutes = CalculateReadingTime(newContent.WordCount);
        
        RegisterDomainEvent(new DocumentUpdatedEvent(Id, userId));
    }
    
    /// <summary>
    /// Publish the document
    /// </summary>
    public void Publish(Guid userId)
    {
        Guard.Against.Default(userId, nameof(userId));
        
        if (IsDeleted)
            throw new InvalidOperationException("Cannot publish a deleted document");
            
        if (Status == DocumentStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived document");
        
        Status = DocumentStatus.Published;
        UpdatedAt = DateTime.UtcNow;
        LastEditedBy = userId;
        
        RegisterDomainEvent(new DocumentPublishedEvent(Id, userId));
    }
    
    /// <summary>
    /// Archive the document
    /// </summary>
    public void Archive(Guid userId)
    {
        Guard.Against.Default(userId, nameof(userId));
        
        if (IsDeleted)
            throw new InvalidOperationException("Cannot archive a deleted document");
        
        Status = DocumentStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        LastEditedBy = userId;
        
        RegisterDomainEvent(new DocumentArchivedEvent(Id, userId));
    }
    
    /// <summary>
    /// Soft delete the document
    /// </summary>
    public void Delete(Guid userId)
    {
        Guard.Against.Default(userId, nameof(userId));
        
        if (IsDeleted)
            throw new InvalidOperationException("Document is already deleted");
        
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        LastEditedBy = userId;
        
        RegisterDomainEvent(new DocumentDeletedEvent(Id, userId));
    }
    
    /// <summary>
    /// Restore a soft-deleted document
    /// </summary>
    public void Restore(Guid userId)
    {
        Guard.Against.Default(userId, nameof(userId));
        
        if (!IsDeleted)
            throw new InvalidOperationException("Document is not deleted");
        
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
        LastEditedBy = userId;
        
        RegisterDomainEvent(new DocumentRestoredEvent(Id, userId));
    }
    
    /// <summary>
    /// Add a tag to the document
    /// </summary>
    public void AddTag(Tag tag)
    {
        Guard.Against.Null(tag, nameof(tag));
        
        if (!_tags.Any(t => t.Id == tag.Id))
        {
            _tags.Add(tag);
        }
    }
    
    /// <summary>
    /// Remove a tag from the document
    /// </summary>
    public void RemoveTag(Tag tag)
    {
        Guard.Against.Null(tag, nameof(tag));
        _tags.Remove(tag);
    }
    
    /// <summary>
    /// Create a version snapshot of the current content
    /// </summary>
    private void CreateVersion()
    {
        var versionNumber = _versions.Count + 1;
        var version = DocumentVersion.Create(
            Id,
            versionNumber,
            Content,
            LastEditedBy ?? CreatedBy,
            "Auto-saved version"
        );
        
        _versions.Add(version);
    }
    
    /// <summary>
    /// Calculate reading time based on average reading speed (200 words per minute)
    /// </summary>
    private static int CalculateReadingTime(int wordCount)
    {
        const int wordsPerMinute = 200;
        return Math.Max(1, (int)Math.Ceiling(wordCount / (double)wordsPerMinute));
    }
}
