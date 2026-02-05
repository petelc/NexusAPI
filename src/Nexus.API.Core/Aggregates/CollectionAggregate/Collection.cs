using Ardalis.GuardClauses;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Events;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.CollectionAggregate;

/// <summary>
/// Collection aggregate root - represents a container for organizing content
/// </summary>
public class Collection : EntityBase<CollectionId>, IAggregateRoot
{
  private readonly List<CollectionItem> _items = new();

  public string Name { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public CollectionId? ParentCollectionId { get; private set; }
  public WorkspaceId WorkspaceId { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public string? Icon { get; private set; }
  public string? Color { get; private set; }
  public int OrderIndex { get; private set; }
  public HierarchyPath HierarchyPath { get; private set; } = null!;
  public bool IsDeleted { get; private set; }
  public DateTime? DeletedAt { get; private set; }

  public IReadOnlyCollection<CollectionItem> Items => _items.AsReadOnly();

  // EF Core constructor
  private Collection() { }

  private Collection(
    CollectionId id,
    string name,
    WorkspaceId workspaceId,
    Guid createdBy,
    CollectionId? parentCollectionId,
    HierarchyPath hierarchyPath,
    string? description,
    string? icon,
    string? color)
  {
    Id = Guard.Against.Null(id, nameof(id));
    Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
    WorkspaceId = Guard.Against.Null(workspaceId, nameof(workspaceId));
    CreatedBy = Guard.Against.Default(createdBy, nameof(createdBy));
    ParentCollectionId = parentCollectionId;
    HierarchyPath = Guard.Against.Null(hierarchyPath, nameof(hierarchyPath));
    Description = description;
    Icon = icon;
    Color = color;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    OrderIndex = 0;
    IsDeleted = false;
  }

  /// <summary>
  /// Creates a new root-level collection
  /// </summary>
  public static Collection CreateRoot(
    string name,
    WorkspaceId workspaceId,
    Guid createdBy,
    string? description = null,
    string? icon = null,
    string? color = null)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.Null(workspaceId, nameof(workspaceId));
    Guard.Against.Default(createdBy, nameof(createdBy));

    if (name.Length > 200)
    {
      throw new DomainException("Collection name cannot exceed 200 characters");
    }

    var id = CollectionId.CreateNew();
    var hierarchyPath = HierarchyPath.CreateRoot(id);

    var collection = new Collection(
      id,
      name,
      workspaceId,
      createdBy,
      null,
      hierarchyPath,
      description,
      icon,
      color);

    collection.RegisterDomainEvent(new CollectionCreatedEvent(id, workspaceId, null));

    return collection;
  }

  /// <summary>
  /// Creates a new child collection under a parent
  /// </summary>
  public static Collection CreateChild(
    string name,
    WorkspaceId workspaceId,
    Guid createdBy,
    CollectionId parentCollectionId,
    HierarchyPath parentHierarchyPath,
    string? description = null,
    string? icon = null,
    string? color = null)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.Null(workspaceId, nameof(workspaceId));
    Guard.Against.Default(createdBy, nameof(createdBy));
    Guard.Against.Null(parentCollectionId, nameof(parentCollectionId));
    Guard.Against.Null(parentHierarchyPath, nameof(parentHierarchyPath));

    if (name.Length > 200)
    {
      throw new DomainException("Collection name cannot exceed 200 characters");
    }

    var id = CollectionId.CreateNew();
    var hierarchyPath = HierarchyPath.CreateChild(parentHierarchyPath, id);

    var collection = new Collection(
      id,
      name,
      workspaceId,
      createdBy,
      parentCollectionId,
      hierarchyPath,
      description,
      icon,
      color);

    collection.RegisterDomainEvent(new CollectionCreatedEvent(id, workspaceId, parentCollectionId));

    return collection;
  }

  /// <summary>
  /// Renames the collection
  /// </summary>
  public void Rename(string newName)
  {
    Guard.Against.NullOrWhiteSpace(newName, nameof(newName));

    if (newName.Length > 200)
    {
      throw new DomainException("Collection name cannot exceed 200 characters");
    }

    if (Name == newName)
    {
      return;
    }

    Name = newName;
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new CollectionRenamedEvent(Id, newName));
  }

  /// <summary>
  /// Updates the collection description
  /// </summary>
  public void UpdateDescription(string? description)
  {
    if (description?.Length > 1000)
    {
      throw new DomainException("Description cannot exceed 1000 characters");
    }

    Description = description;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Sets the collection icon (emoji or identifier)
  /// </summary>
  public void SetIcon(string? icon)
  {
    if (icon?.Length > 50)
    {
      throw new DomainException("Icon identifier cannot exceed 50 characters");
    }

    Icon = icon;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Sets the collection color (hex code)
  /// </summary>
  public void SetColor(string? color)
  {
    if (color != null && !IsValidHexColor(color))
    {
      throw new DomainException("Color must be a valid hex color code (e.g., #FF5733)");
    }

    Color = color;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Adds an item to the collection
  /// </summary>
  public void AddItem(ItemType itemType, Guid itemReferenceId, Guid addedBy)
  {
    Guard.Against.Default(itemReferenceId, nameof(itemReferenceId));
    Guard.Against.Default(addedBy, nameof(addedBy));

    // Check if item already exists
    if (_items.Any(i => i.ItemReferenceId == itemReferenceId))
    {
      throw new DomainException("Item already exists in collection");
    }

    // Prevent circular references for sub-collections
    if (itemType == ItemType.SubCollection && itemReferenceId == Id.Value)
    {
      throw new DomainException("Cannot add collection to itself");
    }

    var order = _items.Count;
    var item = new CollectionItem(
      CollectionItemId.CreateNew(),
      itemType,
      itemReferenceId,
      order,
      addedBy,
      DateTime.UtcNow);

    _items.Add(item);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new ItemAddedToCollectionEvent(Id, itemType, itemReferenceId));
  }

  /// <summary>
  /// Removes an item from the collection
  /// </summary>
  public void RemoveItem(Guid itemReferenceId)
  {
    Guard.Against.Default(itemReferenceId, nameof(itemReferenceId));

    var item = _items.FirstOrDefault(i => i.ItemReferenceId == itemReferenceId);
    if (item == null)
    {
      throw new DomainException("Item not found in collection");
    }

    _items.Remove(item);
    ReorderItems();
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new ItemRemovedFromCollectionEvent(Id, itemReferenceId));
  }

  /// <summary>
  /// Moves an item to a new position in the collection
  /// </summary>
  public void MoveItem(Guid itemReferenceId, int newOrder)
  {
    Guard.Against.Default(itemReferenceId, nameof(itemReferenceId));
    Guard.Against.Negative(newOrder, nameof(newOrder));

    var item = _items.FirstOrDefault(i => i.ItemReferenceId == itemReferenceId);
    if (item == null)
    {
      throw new DomainException("Item not found in collection");
    }

    if (newOrder >= _items.Count)
    {
      throw new DomainException($"Invalid order position. Maximum is {_items.Count - 1}");
    }

    _items.Remove(item);
    _items.Insert(newOrder, item);
    ReorderItems();
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new ItemMovedInCollectionEvent(Id, itemReferenceId, newOrder));
  }

  /// <summary>
  /// Updates the order index (for sibling ordering within parent)
  /// </summary>
  public void UpdateOrderIndex(int newOrderIndex)
  {
    Guard.Against.Negative(newOrderIndex, nameof(newOrderIndex));

    OrderIndex = newOrderIndex;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Soft deletes the collection
  /// </summary>
  public void Delete()
  {
    if (!IsEmpty())
    {
      throw new DomainException("Cannot delete non-empty collection. Remove all items first.");
    }

    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new CollectionDeletedEvent(Id));
  }

  /// <summary>
  /// Checks if the collection is empty
  /// </summary>
  public bool IsEmpty() => !_items.Any();

  /// <summary>
  /// Gets the count of items in the collection
  /// </summary>
  public int GetItemCount() => _items.Count;

  /// <summary>
  /// Checks if this collection would create a circular reference
  /// </summary>
  public bool WouldCreateCircularReference(CollectionId targetCollectionId)
  {
    // Can't move to self
    if (Id == targetCollectionId)
    {
      return true;
    }

    // Can't move to any of our children (would create cycle)
    // This check requires the full hierarchy - handled at repository level
    return false;
  }

  private void ReorderItems()
  {
    for (int i = 0; i < _items.Count; i++)
    {
      _items[i].UpdateOrder(i);
    }
  }

  private static bool IsValidHexColor(string color)
  {
    if (string.IsNullOrWhiteSpace(color))
    {
      return false;
    }

    // Match #RGB or #RRGGBB format
    return System.Text.RegularExpressions.Regex.IsMatch(
      color,
      @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
  }
}
