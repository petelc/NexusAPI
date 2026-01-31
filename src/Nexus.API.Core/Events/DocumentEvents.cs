using Nexus.Core.Aggregates.DocumentAggregate;
using Traxs.SharedKernel;

namespace Nexus.Core.Events;

/// <summary>
/// Domain event raised when a document is updated
/// </summary>
public record DocumentUpdatedEvent(DocumentId DocumentId, Guid UpdatedBy) : DomainEventBase;

/// <summary>
/// Domain event raised when a document is published
/// </summary>
public record DocumentPublishedEvent(DocumentId DocumentId, Guid PublishedBy) : DomainEventBase;

/// <summary>
/// Domain event raised when a document is archived
/// </summary>
public record DocumentArchivedEvent(DocumentId DocumentId, Guid ArchivedBy) : DomainEventBase;

/// <summary>
/// Domain event raised when a document is deleted
/// </summary>
public record DocumentDeletedEvent(DocumentId DocumentId, Guid DeletedBy) : DomainEventBase;

/// <summary>
/// Domain event raised when a document is restored from deletion
/// </summary>
public record DocumentRestoredEvent(DocumentId DocumentId, Guid RestoredBy) : DomainEventBase;
