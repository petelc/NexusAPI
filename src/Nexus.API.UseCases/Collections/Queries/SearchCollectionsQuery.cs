using Nexus.API.UseCases.Collections.DTOs;
using MediatR;
using Ardalis.Result;

namespace Nexus.API.UseCases.Collections.Queries;

public class SearchCollectionsQuery : IRequest<Result<SearchCollectionsResponse>>
{
  public Guid WorkspaceId { get; set; }
  public string SearchTerm { get; set; } = string.Empty;
}

public class SearchCollectionsResponse
{
  public List<CollectionSummaryDto> Collections { get; set; } = new();
}
