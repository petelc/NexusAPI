using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetChildCollectionsQuery
{
  public Guid ParentCollectionId { get; set; }
}

public class GetChildCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
