using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Handlers;

public class AddItemToCollectionHandler
{
  private readonly ICollectionRepository _collectionRepository;
  private readonly ICurrentUserService _currentUserService;

  public AddItemToCollectionHandler(
    ICollectionRepository collectionRepository,
    ICurrentUserService currentUserService)
  {
    _collectionRepository = collectionRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<AddItemToCollectionResponse>> Handle(
    AddItemToCollectionCommand command,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredUserId();
    if (userId == Guid.Empty)
    {
      return Result<AddItemToCollectionResponse>.Unauthorized();
    }

    var collectionId = CollectionId.Create(command.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<AddItemToCollectionResponse>.NotFound("Collection not found");
    }

    // Parse ItemType
    if (!Enum.TryParse<ItemType>(command.ItemType, true, out var itemType))
    {
      return Result<AddItemToCollectionResponse>.Error("Invalid ItemType");
    }

    // Add item
    try
    {
      collection.AddItem(itemType, command.ItemReferenceId, userId);
      await _collectionRepository.UpdateAsync(collection, cancellationToken);

      var addedItem = collection.Items.Last();
      var dto = new CollectionItemDto
      {
        CollectionItemId = addedItem.Id.Value,
        ItemType = addedItem.ItemType.ToString(),
        ItemReferenceId = addedItem.ItemReferenceId,
        Order = addedItem.Order,
        AddedBy = addedItem.AddedBy,
        AddedAt = addedItem.AddedAt
      };

      return Result<AddItemToCollectionResponse>.Success(
        new AddItemToCollectionResponse { Item = dto });
    }
    catch (Exception ex)
    {
      return Result<AddItemToCollectionResponse>.Error(ex.Message);
    }
  }
}
