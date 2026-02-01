namespace Nexus.API.UseCases.Documents.Get;

/// <summary>
/// Response for getting a document by ID
/// </summary>
public record GetDocumentByIdResponse
{
  public Guid DocumentId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Content { get; init; } = string.Empty;
  public string PlainTextContent { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public int WordCount { get; init; }
  public int ReadingTimeMinutes { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
  public UserDto CreatedBy { get; init; } = new();
  public List<TagDto> Tags { get; init; } = new();
  public PermissionsDto Permissions { get; init; } = new();
}

public record UserDto
{
  public Guid UserId { get; init; }
  public string Username { get; init; } = string.Empty;
  public string FullName { get; init; } = string.Empty;
}

public record TagDto
{
  public Guid TagId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Color { get; init; } = string.Empty;
}

public record PermissionsDto
{
  public bool CanEdit { get; init; }
  public bool CanDelete { get; init; }
  public bool CanShare { get; init; }
  public bool IsOwner { get; init; }
}
