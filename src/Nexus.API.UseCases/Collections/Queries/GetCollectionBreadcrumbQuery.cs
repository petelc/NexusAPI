using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetCollectionBreadcrumbQuery
{
  public Guid CollectionId { get; set; }
}

public class GetCollectionBreadcrumbResponse
{
  public List<CollectionSummaryDto> Breadcrumb { get; set; } = new();
}
