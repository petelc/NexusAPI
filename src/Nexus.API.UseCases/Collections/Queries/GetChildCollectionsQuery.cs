using Nexus.API.UseCases.Collections.DTOs;
using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetChildCollectionsQuery : IRequest<Result<GetChildCollectionsResponse>>
{
  public Guid ParentCollectionId { get; set; }
}

public class GetChildCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
