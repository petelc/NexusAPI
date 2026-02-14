using MediatR;
using Ardalis.Result;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Commands;

public class CreateCollectionCommand : IRequest<Result<CreateCollectionResponse>>
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid? ParentCollectionId { get; set; }
  public Guid WorkspaceId { get; set; }
  public string? Icon { get; set; }
  public string? Color { get; set; }
}

public class CreateCollectionResponse
{
  public CollectionDto Collection { get; set; } = null!;
}
