using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetCollectionHierarchyQuery
{
  public Guid WorkspaceId { get; set; }
}

public class GetCollectionHierarchyResponse
{
  public List<CollectionHierarchyDto> Hierarchy { get; set; } = new();
}
