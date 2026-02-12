using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.UseCases.Collections.Handlers;

public class GetCollectionByIdHandler : IRequestHandler<GetCollectionByIdQuery, Result<GetCollectionByIdResponse>>
{
  private readonly ICollectionRepository _collectionRepository;

  public GetCollectionByIdHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<GetCollectionByIdResponse>> Handle(
    GetCollectionByIdQuery query,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(query.CollectionId);
    var collection = await _collectionRepository.GetByIdAsync(collectionId, cancellationToken);

    if (collection == null)
    {
      return Result<GetCollectionByIdResponse>.NotFound("Collection not found");
    }

    var dto = MapToDto(collection);

    return Result<GetCollectionByIdResponse>.Success(
      new GetCollectionByIdResponse { Collection = dto });
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
