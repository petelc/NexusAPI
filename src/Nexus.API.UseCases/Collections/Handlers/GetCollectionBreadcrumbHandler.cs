using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.UseCases.Collections.Handlers;

public class GetCollectionBreadcrumbHandler
{
  private readonly ICollectionRepository _collectionRepository;

  public GetCollectionBreadcrumbHandler(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public async Task<Result<GetCollectionBreadcrumbResponse>> Handle(
    GetCollectionBreadcrumbQuery query,
    CancellationToken cancellationToken)
  {
    var collectionId = CollectionId.Create(query.CollectionId);
    var hierarchy = await _collectionRepository.GetHierarchyAsync(
      collectionId,
      cancellationToken);

    var breadcrumb = hierarchy.Select(MapToSummaryDto).ToList();

    return Result<GetCollectionBreadcrumbResponse>.Success(
      new GetCollectionBreadcrumbResponse { Breadcrumb = breadcrumb });
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
