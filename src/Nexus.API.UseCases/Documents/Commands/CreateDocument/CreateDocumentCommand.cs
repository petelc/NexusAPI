using MediatR;

namespace Nexus.API.UseCases.Documents.Create;

/// <summary>
/// Command to create a new document
/// </summary>
public record CreateDocumentCommand : IRequest<CreateDocumentResponse>
{
  public string Title { get; init; } = string.Empty;
  public string Content { get; init; } = string.Empty;
  public string Status { get; init; } = "draft";
  public Guid? CollectionId { get; init; }
  public List<string> Tags { get; init; } = new();
}
