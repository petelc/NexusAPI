using MediatR;

namespace Nexus.API.UseCases.Documents.List;

/// <summary>
/// Query to list documents with pagination and filtering
/// </summary>
public record ListDocumentsQuery : IRequest<ListDocumentsResponse>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;
  public string? Status { get; init; }
  public Guid? CollectionId { get; init; }
  public string? Tags { get; init; }
  public Guid? CreatedBy { get; init; }
  public string? SortBy { get; init; } = "updatedAt";
  public string? SortOrder { get; init; } = "desc";
  public string? Search { get; init; }
}
