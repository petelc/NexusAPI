namespace Nexus.API.UseCases.Common.DTOs;

/// <summary>
/// Data transfer object for Document
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentRichText { get; set; } = string.Empty;
    public string ContentPlainText { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ReadingTimeMinutes { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? LastEditedBy { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public List<DocumentVersionDto> Versions { get; set; } = new();
}

/// <summary>
/// Data transfer object for Tag
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

/// <summary>
/// Data transfer object for Document Version
/// </summary>
public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
}

/// <summary>
/// Summary DTO for list views
/// </summary>
public class DocumentSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}
