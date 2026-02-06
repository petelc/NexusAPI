using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Handlers;

public class ReorderItemHandler
{
  private readonly ICollectionRepository _collectionRepository;

  public ReorderItemHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<ReorderItemResponse>> Handle(
    ReorderItemCommand command,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(command.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<ReorderItemResponse>.NotFound("Collection not found");
    }

    try
    {
      collection.MoveItem(command.ItemReferenceId, command.NewOrder);
      await _collectionRepository.UpdateAsync(collection, cancellationToken);

      return Result<ReorderItemResponse>.Success(
        new ReorderItemResponse { Success = true });
    }
    catch (Exception ex)
    {
      return Result<ReorderItemResponse>.Error(ex.Message);
    }
  }
}
