using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Create;

/// <summary>
/// Handler for creating a new document
/// </summary>
public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentResponse>
{
  private readonly IDocumentRepository _repository;
  private readonly ICurrentUserService _currentUserService;

  public CreateDocumentHandler(IDocumentRepository repository, ICurrentUserService currentUserService)
  {
    _repository = repository;
    _currentUserService = currentUserService;
  }

  public async Task<CreateDocumentResponse> Handle(
    CreateDocumentCommand request,
    CancellationToken cancellationToken)
  {
    // TODO: Get current user ID from HttpContext/Claims
    // For now using placeholder - implement ICurrentUserService
    var userId = _currentUserService.GetRequiredUserId();

    // Create document aggregate using factory method
    //var title = new Title(request.Title);
    var title = Title.Create(request.Title);
    //var content = new DocumentContent(request.Content); 
    var content = DocumentContent.Create(request.Content);

    var document = Document.Create(title, content, userId);

    // Add tags if provided
    foreach (var tagName in request.Tags)
    {
      //var tag = new Tag(tagName);
      var tag = Tag.Create(tagName);
      document.AddTag(tag);
    }

    // Save to repository
    await _repository.AddAsync(document, cancellationToken);

    return new CreateDocumentResponse
    {
      DocumentId = document.Id.Value,
      Title = document.Title.Value,
      Status = document.Status.ToString().ToLower(),
      CreatedAt = document.CreatedAt,
      CreatedBy = userId.Value.ToString()
    };
  }
}
