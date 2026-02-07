using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Commands;

public class AddItemToCollectionCommand
{
  public Guid CollectionId { get; set; }
  public string ItemType { get; set; } = string.Empty; // Document, Diagram, Snippet, SubCollection
  public Guid ItemReferenceId { get; set; }
}

public class AddItemToCollectionResponse
{
  public CollectionItemDto Item { get; set; } = null!;
}
