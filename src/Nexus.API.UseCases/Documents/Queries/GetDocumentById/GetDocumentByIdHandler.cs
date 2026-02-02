using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Get;

/// <summary>
/// Handler for getting a document by ID
/// </summary>
public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, GetDocumentByIdResponse?>
{
  private readonly IDocumentRepository _repository;
  private readonly ICurrentUserService _currentUserService;

  public GetDocumentByIdHandler(IDocumentRepository repository, ICurrentUserService currentUserService)
  {
    _repository = repository;
    _currentUserService = currentUserService;
  }

  public async Task<GetDocumentByIdResponse?> Handle(
    GetDocumentByIdQuery request,
    CancellationToken cancellationToken)
  {
    var documentId = new DocumentId(request.Id);
    var document = await _repository.GetByIdAsync(documentId, cancellationToken);

    if (document == null)
      return null;


    var userId = _currentUserService.GetRequiredUserId();

    return new GetDocumentByIdResponse
    {
      DocumentId = document.Id.Value,
      Title = document.Title.Value,
      Content = document.Content.RichText,
      PlainTextContent = document.Content.PlainText,
      Status = document.Status.ToString().ToLower(),
      WordCount = document.Content.WordCount,
      ReadingTimeMinutes = CalculateReadingTime(document.Content.WordCount),
      CreatedAt = document.CreatedAt,
      UpdatedAt = document.UpdatedAt,
      CreatedBy = new UserDto
      {
        UserId = document.CreatedBy,
        Username = "user", // TODO: Get from user repository
        FullName = "User Name"
      },
      Tags = document.Tags.Select(t => new TagDto
      {
        TagId = t.Id,
        Name = t.Name,
        Color = t.Color!
      }).ToList(),
      Permissions = new PermissionsDto
      {
        CanEdit = document.CanEdit(new UserId(userId)),
        CanDelete = document.CreatedBy == userId,
        CanShare = document.CreatedBy == userId,
        IsOwner = document.CreatedBy == userId
      }
    };
  }

  private static int CalculateReadingTime(int wordCount)
  {
    // Average reading speed: 200 words per minute
    const int wordsPerMinute = 200;
    return Math.Max(1, (int)Math.Ceiling(wordCount / (double)wordsPerMinute));
  }
}
