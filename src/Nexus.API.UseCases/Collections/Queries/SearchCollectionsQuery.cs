using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class SearchCollectionsQuery
{
  public Guid WorkspaceId { get; set; }
  public string SearchTerm { get; set; } = string.Empty;
}

public class SearchCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
