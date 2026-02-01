namespace Nexus.API.UseCases.Documents.Publish;

/// <summary>
/// Response for publishing a document
/// </summary>
public record PublishDocumentResponse
{
  public Guid DocumentId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public DateTime PublishedAt { get; init; }
  public string PublishedBy { get; init; } = string.Empty;
  public string Message { get; init; } = string.Empty;
}
