using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.CodeSnippetAggregate.Events;

// Domain Events
public class SnippetCreatedEvent : DomainEventBase
{
    public Guid SnippetId { get; set; }
    public Guid? CreatedBy { get; set; }

    public SnippetCreatedEvent(Guid snippetId, Guid? createdBy)
    {
        SnippetId = snippetId;
        CreatedBy = createdBy;
    }
}

public class SnippetUpdatedEvent : DomainEventBase
{
    public Guid SnippetId { get; set; }

    public SnippetUpdatedEvent(Guid snippetId)
    {
        SnippetId = snippetId;
    }
}

public class SnippetForkedEvent : DomainEventBase
{
    public Guid OriginalSnippetId { get; set; }
    public Guid ForkedSnippetId { get; set; }
    public Guid UserId { get; set; }

    public SnippetForkedEvent(Guid originalSnippetId, Guid forkedSnippetId, Guid userId)
    {
        OriginalSnippetId = originalSnippetId;
        ForkedSnippetId = forkedSnippetId;
        UserId = userId;
    }
}

public class SnippetMadePublicEvent : DomainEventBase
{
    public Guid SnippetId { get; set; }

    public SnippetMadePublicEvent(Guid snippetId)
    {
        SnippetId = snippetId;
    }
}

public class SnippetMadePrivateEvent : DomainEventBase
{
    public Guid SnippetId { get; set; }

    public SnippetMadePrivateEvent(Guid snippetId)
    {
        SnippetId = snippetId;
    }
}

public class SnippetDeletedEvent : DomainEventBase
{
    public Guid SnippetId { get; set; }
    public Guid? DeletedBy { get; set; }

    public SnippetDeletedEvent(Guid snippetId, Guid? deletedBy)
    {
        SnippetId = snippetId;
        DeletedBy = deletedBy;
    }
}