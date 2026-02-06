using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Handlers;

public class RemoveItemFromCollectionHandler
{
  private readonly ICollectionRepository _collectionRepository;

  public RemoveItemFromCollectionHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<RemoveItemFromCollectionResponse>> Handle(
    RemoveItemFromCollectionCommand command,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(command.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<RemoveItemFromCollectionResponse>.NotFound("Collection not found");
    }

    collection.RemoveItem(command.ItemReferenceId);
    await _collectionRepository.UpdateAsync(collection, cancellationToken);

    return Result<RemoveItemFromCollectionResponse>.Success(
      new RemoveItemFromCollectionResponse { Success = true });
  }
}
