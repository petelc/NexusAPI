using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Publish;

/// <summary>
/// Handler for publishing a document
/// </summary>
public class PublishDocumentHandler : IRequestHandler<PublishDocumentCommand, PublishDocumentResponse>
{
  private readonly IDocumentRepository _repository;

  public PublishDocumentHandler(IDocumentRepository repository)
  {
    _repository = repository;
  }

  public async Task<PublishDocumentResponse> Handle(
    PublishDocumentCommand request,
    CancellationToken cancellationToken)
  {
    var documentId = new DocumentId(request.Id);
    var document = await _repository.GetByIdAsync(documentId, cancellationToken);

    if (document == null)
      throw new InvalidOperationException($"Document {request.Id} not found");

    // TODO: Get current user ID from HttpContext/Claims
    var userId = UserId.Create(Guid.NewGuid());

    // Publish the document
    document.Publish(userId);

    // Save changes
    await _repository.UpdateAsync(document, cancellationToken);

    return new PublishDocumentResponse
    {
      DocumentId = document.Id.Value,
      Title = document.Title.Value,
      Status = document.Status.ToString().ToLower(),
      PublishedAt = document.UpdatedAt,
      PublishedBy = userId.Value.ToString(),
      Message = "Document published successfully"
    };
  }
}
