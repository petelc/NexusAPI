using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Handlers;

public class DeleteCollectionHandler
{
  private readonly ICollectionRepository _collectionRepository;

  public DeleteCollectionHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<DeleteCollectionResponse>> Handle(
    DeleteCollectionCommand command,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(command.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<DeleteCollectionResponse>.NotFound("Collection not found");
    }

    // Check if collection is empty
    if (!command.Force && !collection.IsEmpty())
    {
      return Result<DeleteCollectionResponse>.Error(
        "Collection must be empty before deletion. Use Force=true to override.");
    }

    // Soft delete
    collection.Delete();
    await _collectionRepository.UpdateAsync(collection, cancellationToken);

    return Result<DeleteCollectionResponse>.Success(
      new DeleteCollectionResponse
      {
        Success = true,
        Message = "Collection deleted successfully"
      });
  }
}
