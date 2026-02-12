using Nexus.API.UseCases.Collections.DTOs;
using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetCollectionByIdQuery : IRequest<Result<GetCollectionByIdResponse>>
{
  public Guid CollectionId { get; set; }
  public bool IncludeItems { get; set; } = true;
}

public class GetCollectionByIdResponse
{
  public CollectionDto? Collection { get; set; }
}
