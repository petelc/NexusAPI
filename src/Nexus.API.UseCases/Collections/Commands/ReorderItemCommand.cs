namespace Nexus.API.UseCases.Collections.Commands;

public class ReorderItemCommand
{
  public Guid CollectionId { get; set; }
  public Guid ItemReferenceId { get; set; }
  public int NewOrder { get; set; }
}

public class ReorderItemResponse
{
  public bool Success { get; set; }
}
