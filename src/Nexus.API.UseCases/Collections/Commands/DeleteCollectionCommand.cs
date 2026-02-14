using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Commands;

public class DeleteCollectionCommand : IRequest<Result>
{
  public Guid CollectionId { get; set; }
  public bool Force { get; set; } // Force delete even if not empty
}

public class DeleteCollectionResponse
{
  public bool Success { get; set; }
  public string? Message { get; set; }
}
