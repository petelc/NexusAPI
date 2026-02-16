using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.UseCases.Documents.List;

/// <summary>
/// Handler for listing documents with pagination
/// </summary>
public class ListDocumentsHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResponse>
{
  private readonly IDocumentRepository _repository;
  private readonly IUserRepository _userRepository;

  public ListDocumentsHandler(IDocumentRepository repository, IUserRepository userRepository)
  {
    _repository = repository;
    _userRepository = userRepository;
  }

  public async Task<ListDocumentsResponse> Handle(
    ListDocumentsQuery request,
    CancellationToken cancellationToken)
  {
    IEnumerable<Document> documents;

    // Use search if provided, otherwise load all
    if (!string.IsNullOrEmpty(request.Search))
    {
      documents = await _repository.SearchAsync(request.Search, cancellationToken);
    }
    else
    {
      documents = await _repository.ListAsync(cancellationToken);
    }

    // Exclude soft-deleted
    documents = documents.Where(d => !d.IsDeleted);

    // Filter by status
    if (!string.IsNullOrEmpty(request.Status) &&
        Enum.TryParse<DocumentStatus>(request.Status, ignoreCase: true, out var statusFilter))
    {
      documents = documents.Where(d => d.Status == statusFilter);
    }

    // Filter by createdBy
    if (request.CreatedBy.HasValue)
    {
      documents = documents.Where(d => d.CreatedBy == request.CreatedBy.Value);
    }

    // Filter by tags (comma-separated)
    if (!string.IsNullOrEmpty(request.Tags))
    {
      var tagNames = request.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      documents = documents.Where(d => tagNames.Any(t => d.Tags.Any(dt => dt.Name.Equals(t, StringComparison.OrdinalIgnoreCase))));
    }

    // Apply sorting
    documents = (request.SortBy?.ToLowerInvariant(), request.SortOrder?.ToLowerInvariant()) switch
    {
      ("createdat", "asc") => documents.OrderBy(d => d.CreatedAt),
      ("createdat", _) => documents.OrderByDescending(d => d.CreatedAt),
      ("title", "asc") => documents.OrderBy(d => d.Title.Value),
      ("title", "desc") => documents.OrderByDescending(d => d.Title.Value),
      ("updatedat", "asc") => documents.OrderBy(d => d.UpdatedAt),
      _ => documents.OrderByDescending(d => d.UpdatedAt),
    };

    // Materialize for count and pagination
    var allDocuments = documents.ToList();
    var totalItems = allDocuments.Count;
    var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
    var skip = (request.Page - 1) * request.PageSize;

    var pagedDocuments = allDocuments
      .Skip(skip)
      .Take(request.PageSize)
      .ToList();

    // Fetch users for all distinct CreatedBy IDs
    var userIds = pagedDocuments.Select(d => d.CreatedBy).Distinct().ToList();
    var users = new Dictionary<Guid, Core.Aggregates.UserAggregate.User>();
    foreach (var uid in userIds)
    {
      var user = await _userRepository.GetByIdAsync(UserId.From(uid), cancellationToken);
      if (user != null)
        users[uid] = user;
    }

    var data = pagedDocuments.Select(d =>
    {
      users.TryGetValue(d.CreatedBy, out var createdByUser);
      return new DocumentSummaryDto
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
          Username = createdByUser?.Username ?? "Unknown",
          FullName = createdByUser != null ? $"{createdByUser.FullName.FirstName} {createdByUser.FullName.LastName}" : "Unknown User"
        },
        Tags = d.Tags.Select(t => t.Name).ToList()
      };
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
