using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Commands;

public class UpdateCollectionCommand
{
  public Guid CollectionId { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }
  public string? Icon { get; set; }
  public string? Color { get; set; }
}

public class UpdateCollectionResponse
{
  public CollectionDto Collection { get; set; } = null!;
}
