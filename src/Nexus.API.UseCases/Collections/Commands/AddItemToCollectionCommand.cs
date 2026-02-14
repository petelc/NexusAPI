using Nexus.API.UseCases.Collections.DTOs;

using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Commands;

public class AddItemToCollectionCommand : IRequest<Result>
{
  public Guid CollectionId { get; set; }
  public string ItemType { get; set; } = string.Empty; // Document, Diagram, Snippet, SubCollection
  public Guid ItemReferenceId { get; set; }
}

public class AddItemToCollectionResponse
{
  public CollectionItemDto Item { get; set; } = null!;
}
