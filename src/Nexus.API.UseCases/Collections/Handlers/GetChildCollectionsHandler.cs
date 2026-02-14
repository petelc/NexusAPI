using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.UseCases.Collections.Handlers;

public class GetChildCollectionsHandler : IRequestHandler<GetChildCollectionsQuery, Result<GetChildCollectionsResponse>>
{
  private readonly ICollectionRepository _collectionRepository;

  public GetChildCollectionsHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<GetChildCollectionsResponse>> Handle(
    GetChildCollectionsQuery query,
    CancellationToken cancellationToken)
  {
    var parentId = CollectionId.Create(query.ParentCollectionId);
    var collections = await _collectionRepository.GetChildCollectionsAsync(
      parentId,
      cancellationToken);

    var dtos = collections.Select(MapToSummaryDto).ToList();

    return Result<GetChildCollectionsResponse>.Success(
      new GetChildCollectionsResponse { Collections = dtos });
  }

  private static CollectionSummaryDto MapToSummaryDto(Collection collection)
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
}
