using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections;

/// <summary>
/// Extension methods for mapping Collection entities to DTOs
/// </summary>
public static class CollectionMappingExtensions
{
  public static CollectionDto ToDto(this Collection collection)
  {
    return new CollectionDto
    {
      CollectionId = collection.Id.Value,
      Name = collection.Name,
      Description = collection.Description,
      ParentCollectionId = collection.ParentCollectionId?.Value,
      WorkspaceId = collection.WorkspaceId.Value,
      CreatedBy = collection.CreatedBy,
      CreatedAt = collection.CreatedAt,
      UpdatedAt = collection.UpdatedAt,
      Icon = collection.Icon,
      Color = collection.Color,
      OrderIndex = collection.OrderIndex,
      HierarchyLevel = collection.HierarchyPath.Level,
      HierarchyPath = collection.HierarchyPath.Value,
      ItemCount = collection.GetItemCount(),
      Items = collection.Items.Select(item => item.ToDto()).ToList()
    };
  }

  public static CollectionSummaryDto ToSummaryDto(this Collection collection)
  {
    return new CollectionSummaryDto
    {
      CollectionId = collection.Id.Value,
      Name = collection.Name,
      Icon = collection.Icon,
      Color = collection.Color,
      ParentCollectionId = collection.ParentCollectionId?.Value,
      HierarchyLevel = collection.HierarchyPath.Level,
      ItemCount = collection.GetItemCount(),
      UpdatedAt = collection.UpdatedAt
    };
  }

  public static CollectionItemDto ToDto(this CollectionItem item)
  {
    return new CollectionItemDto
    {
      CollectionItemId = item.Id.Value,
      ItemType = item.ItemType.ToString(),
      ItemReferenceId = item.ItemReferenceId,
      Order = item.Order,
      AddedBy = item.AddedBy,
      AddedAt = item.AddedAt
    };
  }

  public static CollectionHierarchyDto ToHierarchyDto(
    this Collection collection,
    List<Collection> allCollections)
  {
    var children = allCollections
      .Where(c => c.ParentCollectionId == collection.Id)
      .Select(c => c.ToHierarchyDto(allCollections))
      .ToList();

    return new CollectionHierarchyDto
    {
      CollectionId = collection.Id.Value,
      Name = collection.Name,
      Icon = collection.Icon,
      Color = collection.Color,
      HierarchyLevel = collection.HierarchyPath.Level,
      ItemCount = collection.GetItemCount(),
      Children = children
    };
  }
}
