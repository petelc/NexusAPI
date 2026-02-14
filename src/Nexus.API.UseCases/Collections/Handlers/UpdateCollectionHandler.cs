using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Handlers;

/// <summary>
/// Handler for updating a collection
/// </summary>
public class UpdateCollectionHandler : IRequestHandler<UpdateCollectionCommand, Result<UpdateCollectionResponse>>
{
  private readonly ICollectionRepository _collectionRepository;

  public UpdateCollectionHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<UpdateCollectionResponse>> Handle(
    UpdateCollectionCommand command,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(command.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<UpdateCollectionResponse>.NotFound("Collection not found");
    }

    // Update name
    if (!string.IsNullOrEmpty(command.Name) && command.Name != collection.Name)
    {
      // Check for duplicate name
      var nameExists = await _collectionRepository.ExistsWithNameAsync(
        collection.WorkspaceId,
        command.Name,
        collection.ParentCollectionId,
        collectionId,
        cancellationToken);

      if (nameExists)
      {
        return Result<UpdateCollectionResponse>.Error(
          "A collection with this name already exists in the same location");
      }

      collection.Rename(command.Name);
    }

    // Update description
    if (command.Description != null)
    {
      collection.UpdateDescription(command.Description);
    }

    // Update icon
    if (command.Icon != null)
    {
      collection.SetIcon(command.Icon);
    }

    // Update color
    if (command.Color != null)
    {
      collection.SetColor(command.Color);
    }

    await _collectionRepository.UpdateAsync(collection, cancellationToken);

    var dto = MapToDto(collection);

    return Result<UpdateCollectionResponse>.Success(
      new UpdateCollectionResponse { Collection = dto });
  }

  private static CollectionDto MapToDto(Collection collection)
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
      Items = collection.Items.Select(item => new CollectionItemDto
      {
        CollectionItemId = item.Id.Value,
        ItemType = item.ItemType.ToString(),
        ItemReferenceId = item.ItemReferenceId,
        Order = item.Order,
        AddedBy = item.AddedBy,
        AddedAt = item.AddedAt
      }).ToList()
    };
  }
}
