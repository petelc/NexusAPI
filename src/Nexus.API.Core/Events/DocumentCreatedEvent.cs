using Nexus.Core.Aggregates.DocumentAggregate;
using Traxs.SharedKernel;

namespace Nexus.Core.Events;

/// <summary>
/// Domain event raised when a document is created
/// </summary>
public record DocumentCreatedEvent(DocumentId DocumentId, Guid CreatedBy) : DomainEventBase;
