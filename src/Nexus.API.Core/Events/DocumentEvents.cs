using Nexus.API.Core.Aggregates.DocumentAggregate;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Events;

/// <summary>
/// Domain event raised when a document is updated
/// </summary>
public class DocumentUpdatedEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid UpdatedBy { get; init; }

    public DocumentUpdatedEvent(DocumentId documentId, Guid updatedBy)
    {
        DocumentId = documentId;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// Domain event raised when a document is published
/// </summary>
public class DocumentPublishedEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid PublishedBy { get; init; }

    public DocumentPublishedEvent(DocumentId documentId, Guid publishedBy)
    {
        DocumentId = documentId;
        PublishedBy = publishedBy;
    }
}

/// <summary>
/// Domain event raised when a document is archived
/// </summary>
public class DocumentArchivedEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid ArchivedBy { get; init; }

    public DocumentArchivedEvent(DocumentId documentId, Guid archivedBy)
    {
        DocumentId = documentId;
        ArchivedBy = archivedBy;
    }
}

/// <summary>
/// Domain event raised when a document is deleted
/// </summary>
public class DocumentDeletedEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid DeletedBy { get; init; }

    public DocumentDeletedEvent(DocumentId documentId, Guid deletedBy)
    {
        DocumentId = documentId;
        DeletedBy = deletedBy;
    }
}

/// <summary>
/// Domain event raised when a document is restored from deletion
/// </summary>
public class DocumentRestoredEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid RestoredBy { get; init; }

    public DocumentRestoredEvent(DocumentId documentId, Guid restoredBy)
    {
        DocumentId = documentId;
        RestoredBy = restoredBy;
    }
}
