namespace Nexus.API.UseCases.Collections.Commands;

public class RemoveItemFromCollectionCommand
{
  public Guid CollectionId { get; set; }
  public Guid ItemReferenceId { get; set; }
}

public class RemoveItemFromCollectionResponse
{
  public bool Success { get; set; }
}
