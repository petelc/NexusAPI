using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetCollectionByIdQuery
{
  public Guid CollectionId { get; set; }
  public bool IncludeItems { get; set; } = true;
}

public class GetCollectionByIdResponse
{
  public CollectionDto? Collection { get; set; }
}
