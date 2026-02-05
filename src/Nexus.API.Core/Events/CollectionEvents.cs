using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Events;

/// <summary>
/// Event raised when a collection is created
/// </summary>
public class CollectionCreatedEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public WorkspaceId WorkspaceId { get; init; }
  public CollectionId? ParentCollectionId { get; init; }

  public CollectionCreatedEvent(CollectionId collectionId, WorkspaceId workspaceId, CollectionId? parentCollectionId)
  {
    CollectionId = collectionId;
    WorkspaceId = workspaceId;
    ParentCollectionId = parentCollectionId;
  }
}

/// <summary>
/// Event raised when a collection is renamed
/// </summary>
public class CollectionRenamedEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public string NewName { get; init; }

  public CollectionRenamedEvent(CollectionId collectionId, string newName)
  {
    CollectionId = collectionId;
    NewName = newName;
  }
}

/// <summary>
/// Event raised when a collection is moved to a new parent
/// </summary>
public class CollectionMovedEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public CollectionId? OldParentId { get; init; }
  public CollectionId? NewParentId { get; init; }

  public CollectionMovedEvent(CollectionId collectionId, CollectionId? oldParentId, CollectionId? newParentId)
  {
    CollectionId = collectionId;
    OldParentId = oldParentId;
    NewParentId = newParentId;
  }
}

/// <summary>
/// Event raised when an item is added to a collection
/// </summary>
public class ItemAddedToCollectionEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public ItemType ItemType { get; init; }
  public Guid ItemReferenceId { get; init; }

  public ItemAddedToCollectionEvent(CollectionId collectionId, ItemType itemType, Guid itemReferenceId)
  {
    CollectionId = collectionId;
    ItemType = itemType;
    ItemReferenceId = itemReferenceId;
  }
}

/// <summary>
/// Event raised when an item is removed from a collection
/// </summary>
public class ItemRemovedFromCollectionEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public Guid ItemReferenceId { get; init; }

  public ItemRemovedFromCollectionEvent(CollectionId collectionId, Guid itemReferenceId)
  {
    CollectionId = collectionId;
    ItemReferenceId = itemReferenceId;
  }
}

/// <summary>
/// Event raised when an item is moved within a collection
/// </summary>
public class ItemMovedInCollectionEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }
  public Guid ItemReferenceId { get; init; }
  public int NewOrder { get; init; }

  public ItemMovedInCollectionEvent(CollectionId collectionId, Guid itemReferenceId, int newOrder)
  {
    CollectionId = collectionId;
    ItemReferenceId = itemReferenceId;
    NewOrder = newOrder;
  }
}

/// <summary>
/// Event raised when a collection is deleted
/// </summary>
public class CollectionDeletedEvent : DomainEventBase
{
  public CollectionId CollectionId { get; init; }

  public CollectionDeletedEvent(CollectionId collectionId)
  {
    CollectionId = collectionId;
  }
}
