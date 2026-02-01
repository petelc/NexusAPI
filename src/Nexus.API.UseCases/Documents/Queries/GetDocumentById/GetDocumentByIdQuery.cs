using MediatR;

namespace Nexus.API.UseCases.Documents.Get;

/// <summary>
/// Query to get a document by ID
/// </summary>
public record GetDocumentByIdQuery : IRequest<GetDocumentByIdResponse?>
{
  public Guid Id { get; init; }
  public bool IncludeVersions { get; init; }
  public bool IncludePermissions { get; init; } = true;
}
