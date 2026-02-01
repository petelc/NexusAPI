using MediatR;

namespace Nexus.API.UseCases.Documents.Publish;

/// <summary>
/// Command to publish a document
/// </summary>
public record PublishDocumentCommand : IRequest<PublishDocumentResponse>
{
  public Guid Id { get; init; }
}
