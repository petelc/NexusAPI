namespace Nexus.API.UseCases.CodeSnippets.DTOs;

/// <summary>
/// Request DTO for creating a new code snippet
/// </summary>
public class CreateSnippetRequest
{
  public string Title { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public string? LanguageVersion { get; set; }
  public string? Description { get; set; }
  public List<string>? Tags { get; set; }
}

/// <summary>
/// Request DTO for updating an existing code snippet
/// </summary>
public class UpdateSnippetRequest
{
  public string? Title { get; set; }
  public string? Code { get; set; }
  public string? Description { get; set; }
  public List<string>? Tags { get; set; }
}

/// <summary>
/// Request DTO for forking a code snippet
/// </summary>
public class ForkSnippetRequest
{
  public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Full code snippet response DTO (includes code)
/// Used for: GetById, Create, Update, Fork, Publish, Unpublish
/// </summary>
public class CodeSnippetDto
{
  public Guid SnippetId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public string? LanguageVersion { get; set; }
  public string? Description { get; set; }
  public int LineCount { get; set; }
  public int CharacterCount { get; set; }
  public bool IsPublic { get; set; }
  public int ForkCount { get; set; }
  public int ViewCount { get; set; }
  public Guid? OriginalSnippetId { get; set; }
  public UserInfoDto CreatedBy { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public List<TagDto> Tags { get; set; } = new();
}

/// <summary>
/// Alias for CodeSnippetDto - used for single snippet responses
/// </summary>
public class CodeSnippetResponseDto : CodeSnippetDto
{
}

/// <summary>
/// Lightweight snippet info for list views (excludes code for performance)
/// Used for: GetPublic, GetMy, GetByLanguage, GetByTag, Search
/// </summary>
public class CodeSnippetListItemDto
{
  public Guid SnippetId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public string? Description { get; set; }
  public int LineCount { get; set; }
  public bool IsPublic { get; set; }
  public int ForkCount { get; set; }
  public int ViewCount { get; set; }
  public UserInfoDto CreatedBy { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Paginated result for snippet lists
/// </summary>
public class CodeSnippetPagedResultDto
{
  public List<CodeSnippetListItemDto> Items { get; set; } = new();
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}

/// <summary>
/// User information DTO
/// </summary>
public class UserInfoDto
{
  public Guid UserId { get; set; }
  public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Tag information DTO
/// </summary>
public class TagDto
{
  public Guid TagId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Color { get; set; }
  public int UsageCount { get; set; }

  // Parameterless constructor
  public TagDto()
  {
  }

  // Constructor for convenience
  public TagDto(Guid tagId, string name, string? color)
  {
    TagId = tagId;
    Name = name;
    Color = color;
    UsageCount = 0;
  }
}
