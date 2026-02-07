using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Commands;

public class MoveCollectionCommand
{
  public Guid CollectionId { get; set; }
  public Guid? NewParentCollectionId { get; set; } // null = move to root
}

public class MoveCollectionResponse
{
  public CollectionDto Collection { get; set; } = null!;
}
