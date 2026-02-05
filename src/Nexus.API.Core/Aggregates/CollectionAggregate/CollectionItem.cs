using Ardalis.GuardClauses;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Aggregates.CollectionAggregate;

/// <summary>
/// Represents an item within a collection
/// </summary>
public class CollectionItem : EntityBase<CollectionItemId>
{
  private int _order;

  public ItemType ItemType { get; private set; }
  public Guid ItemReferenceId { get; private set; }
  public int Order
  {
    get => _order;
    private set => _order = value;
  }
  public Guid AddedBy { get; private set; }
  public DateTime AddedAt { get; private set; }

  // EF Core constructor
  private CollectionItem() { }

  internal CollectionItem(
    CollectionItemId id,
    ItemType itemType,
    Guid itemReferenceId,
    int order,
    Guid addedBy,
    DateTime addedAt)
  {
    Id = Guard.Against.Null(id, nameof(id));
    ItemType = itemType;
    ItemReferenceId = Guard.Against.Default(itemReferenceId, nameof(itemReferenceId));
    _order = Guard.Against.Negative(order, nameof(order));
    AddedBy = Guard.Against.Default(addedBy, nameof(addedBy));
    AddedAt = addedAt;
  }

  /// <summary>
  /// Updates the order position of this item
  /// </summary>
  internal void UpdateOrder(int newOrder)
  {
    Guard.Against.Negative(newOrder, nameof(newOrder));
    _order = newOrder;
  }
}
