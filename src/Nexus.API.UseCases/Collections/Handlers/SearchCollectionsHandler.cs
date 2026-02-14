using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.UseCases.Collections.Handlers;

public class SearchCollectionsHandler : IRequestHandler<SearchCollectionsQuery, Result<SearchCollectionsResponse>>
{
  private readonly ICollectionRepository _collectionRepository;

  public SearchCollectionsHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<SearchCollectionsResponse>> Handle(
    SearchCollectionsQuery query,
    CancellationToken cancellationToken)
  {
    var workspaceId = WorkspaceId.Create(query.WorkspaceId);
    var collections = await _collectionRepository.SearchAsync(
      workspaceId,
      query.SearchTerm,
      cancellationToken);

    var dtos = collections.Select(MapToSummaryDto).ToList();

    return Result<SearchCollectionsResponse>.Success(
      new SearchCollectionsResponse { Collections = dtos });
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
