using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Commands;

public class RemoveItemFromCollectionCommand : IRequest<Result>
{
  public Guid CollectionId { get; set; }
  public Guid ItemReferenceId { get; set; }
}

public class RemoveItemFromCollectionResponse
{
  public bool Success { get; set; }
}
