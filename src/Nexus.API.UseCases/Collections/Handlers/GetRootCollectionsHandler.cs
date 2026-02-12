using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.UseCases.Collections.Handlers;

public class GetRootCollectionsHandler : IRequestHandler<GetRootCollectionsQuery, Result<GetRootCollectionsResponse>>
{
  private readonly ICollectionRepository _collectionRepository;

  public GetRootCollectionsHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<GetRootCollectionsResponse>> Handle(
    GetRootCollectionsQuery query,
    CancellationToken cancellationToken)
  {
    var workspaceId = WorkspaceId.Create(query.WorkspaceId);
    var collections = await _collectionRepository.GetRootCollectionsAsync(
      workspaceId,
      cancellationToken);

    var dtos = collections.Select(MapToSummaryDto).ToList();

    return Result<GetRootCollectionsResponse>.Success(
      new GetRootCollectionsResponse { Collections = dtos });
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
