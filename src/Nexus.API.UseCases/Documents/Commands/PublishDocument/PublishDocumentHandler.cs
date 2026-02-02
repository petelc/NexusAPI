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
  private readonly ICurrentUserService _currentUserService;

  public PublishDocumentHandler(IDocumentRepository repository, ICurrentUserService currentUserService)
  {
    _repository = repository;
    _currentUserService = currentUserService;
  }

  public async Task<PublishDocumentResponse> Handle(
    PublishDocumentCommand request,
    CancellationToken cancellationToken)
  {
    var documentId = new DocumentId(request.Id);
    var document = await _repository.GetByIdAsync(documentId, cancellationToken);

    if (document == null)
      throw new InvalidOperationException($"Document {request.Id} not found");


    var userId = _currentUserService.GetRequiredUserId();

    // Publish the document
    document.Publish(userId.Value);

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
