using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetRootCollectionsQuery
{
  public Guid WorkspaceId { get; set; }
}

public class GetRootCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
