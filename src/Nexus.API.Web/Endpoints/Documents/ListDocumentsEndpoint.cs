using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Documents.List;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// List documents endpoint with pagination and filtering
/// GET /api/documents
/// </summary>
public class ListDocumentsEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public ListDocumentsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/documents");
    AllowAnonymous(); // TODO: Add authentication

    Description(b => b
      .WithTags("Documents")
      .WithSummary("List documents")
      .WithDescription(@"
Returns a paginated list of documents with filtering options.

Query Parameters:
- page: Page number (default: 1)
- pageSize: Page size (default: 20, max: 100)
- status: Filter by status (draft, published, archived)
- collectionId: Filter by collection ID
- tags: Comma-separated tag names
- createdBy: Filter by creator user ID
- sortBy: Sort field (createdAt, updatedAt, title)
- sortOrder: Sort order (asc, desc)
- search: Full-text search query

Example: GET /documents?status=published&pageSize=50&search=api
"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    // Get query parameters
    var page = Query<int?>("page", isRequired: false) ?? 1;
    var pageSize = Query<int?>("pageSize", isRequired: false) ?? 20;
    var status = Query<string>("status", isRequired: false);
    var collectionId = Query<Guid?>("collectionId", isRequired: false);
    var tags = Query<string>("tags", isRequired: false);
    var createdBy = Query<Guid?>("createdBy", isRequired: false);
    var sortBy = Query<string>("sortBy", isRequired: false) ?? "updatedAt";
    var sortOrder = Query<string>("sortOrder", isRequired: false) ?? "desc";
    var search = Query<string>("search", isRequired: false);

    // Validate page size
    if (pageSize > 100) pageSize = 100;
    if (pageSize < 1) pageSize = 20;

    var query = new ListDocumentsQuery
    {
      Page = page,
      PageSize = pageSize,
      Status = status,
      CollectionId = collectionId,
      Tags = tags,
      CreatedBy = createdBy,
      SortBy = sortBy,
      SortOrder = sortOrder,
      Search = search
    };

    var result = await _mediator.Send(query, ct);

    await HttpContext.Response.WriteAsJsonAsync(result, ct);
  }
}
