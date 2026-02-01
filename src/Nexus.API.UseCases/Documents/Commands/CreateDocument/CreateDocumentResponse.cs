namespace Nexus.API.UseCases.Documents.Create;

/// <summary>
/// Response for document creation
/// </summary>
public record CreateDocumentResponse
{
  public Guid DocumentId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
  public string CreatedBy { get; init; } = string.Empty;
}
