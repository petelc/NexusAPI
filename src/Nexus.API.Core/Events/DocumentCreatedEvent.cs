using Nexus.API.Core.Aggregates.DocumentAggregate;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Events;

/// <summary>
/// Domain event raised when a document is created
/// </summary>
public class DocumentCreatedEvent : DomainEventBase
{
    public DocumentId DocumentId { get; init; }
    public Guid CreatedBy { get; init; }

    public DocumentCreatedEvent(DocumentId documentId, Guid createdBy)
    {
        DocumentId = documentId;
        CreatedBy = createdBy;
    }
}
