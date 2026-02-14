using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Commands;

public class ReorderItemCommand : IRequest<Result>
{
  public Guid CollectionId { get; set; }
  public Guid ItemReferenceId { get; set; }
  public int NewOrder { get; set; }
}

public class ReorderItemResponse
{
  public bool Success { get; set; }
}
