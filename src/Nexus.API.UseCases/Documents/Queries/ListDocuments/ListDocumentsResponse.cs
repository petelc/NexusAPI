namespace Nexus.API.UseCases.Documents.List;

/// <summary>
/// Response for listing documents with pagination
/// </summary>
public record ListDocumentsResponse
{
  public List<DocumentSummaryDto> Data { get; init; } = new();
  public PaginationDto Pagination { get; init; } = new();
  public LinksDto Links { get; init; } = new();
}

public record DocumentSummaryDto
{
  public Guid DocumentId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Excerpt { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public int WordCount { get; init; }
  public int ReadingTimeMinutes { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
  public UserDto CreatedBy { get; init; } = new();
  public List<string> Tags { get; init; } = new();
}

public record UserDto
{
  public Guid UserId { get; init; }
  public string Username { get; init; } = string.Empty;
  public string FullName { get; init; } = string.Empty;
}

public record PaginationDto
{
  public int CurrentPage { get; init; }
  public int PageSize { get; init; }
  public int TotalPages { get; init; }
  public int TotalItems { get; init; }
  public bool HasNextPage { get; init; }
  public bool HasPreviousPage { get; init; }
}

public record LinksDto
{
  public string Self { get; init; } = string.Empty;
  public string? First { get; init; }
  public string? Last { get; init; }
  public string? Next { get; init; }
  public string? Previous { get; init; }
}
