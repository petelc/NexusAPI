using Nexus.API.UseCases.Collections.DTOs;
using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Queries;

public class GetRootCollectionsQuery : IRequest<Result<GetRootCollectionsResponse>>
{
  public Guid WorkspaceId { get; set; }
}

public class GetRootCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
