using MediatR;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.List;

/// <summary>
/// Handler for listing documents with pagination
/// </summary>
public class ListDocumentsHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResponse>
{
  private readonly IDocumentRepository _repository;

  public ListDocumentsHandler(IDocumentRepository repository)
  {
    _repository = repository;
  }

  public async Task<ListDocumentsResponse> Handle(
    ListDocumentsQuery request,
    CancellationToken cancellationToken)
  {
    // TODO: Apply filters and pagination using specifications
    var allDocuments = await _repository.ListAsync(cancellationToken);

    // Apply search if provided
    if (!string.IsNullOrEmpty(request.Search))
    {
      var searchResults = await _repository.SearchAsync(request.Search, cancellationToken);
      allDocuments = searchResults.ToList();
    }

    // Calculate pagination
    var totalItems = allDocuments.Count;
    var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
    var skip = (request.Page - 1) * request.PageSize;

    var pagedDocuments = allDocuments
      .Skip(skip)
      .Take(request.PageSize)
      .ToList();

    var data = pagedDocuments.Select(d => new DocumentSummaryDto
    {
      DocumentId = d.Id.Value,
      Title = d.Title.Value,
      Excerpt = d.Content.PlainText.Length > 200
        ? d.Content.PlainText.Substring(0, 200) + "..."
        : d.Content.PlainText,
      Status = d.Status.ToString().ToLower(),
      WordCount = d.Content.WordCount,
      ReadingTimeMinutes = CalculateReadingTime(d.Content.WordCount),
      CreatedAt = d.CreatedAt,
      UpdatedAt = d.UpdatedAt,
      CreatedBy = new UserDto
      {
        UserId = d.CreatedBy,
        Username = "user", // TODO: Get from user repository
        FullName = "User Name"
      },
      Tags = d.Tags.Select(t => t.Name).ToList()
    }).ToList();

    return new ListDocumentsResponse
    {
      Data = data,
      Pagination = new PaginationDto
      {
        CurrentPage = request.Page,
        PageSize = request.PageSize,
        TotalPages = totalPages,
        TotalItems = totalItems,
        HasNextPage = request.Page < totalPages,
        HasPreviousPage = request.Page > 1
      },
      Links = new LinksDto
      {
        Self = $"/api/documents?page={request.Page}&pageSize={request.PageSize}",
        First = $"/api/documents?page=1&pageSize={request.PageSize}",
        Last = $"/api/documents?page={totalPages}&pageSize={request.PageSize}",
        Next = request.Page < totalPages
          ? $"/api/documents?page={request.Page + 1}&pageSize={request.PageSize}"
          : null,
        Previous = request.Page > 1
          ? $"/api/documents?page={request.Page - 1}&pageSize={request.PageSize}"
          : null
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
