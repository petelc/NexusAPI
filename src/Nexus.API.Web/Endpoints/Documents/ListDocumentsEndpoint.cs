using FastEndpoints;
using MediatR;
using Nexus.UseCases.Documents.Queries.ListDocuments;

namespace Nexus.Web.Endpoints.Documents;

/// <summary>
/// Request model for listing documents
/// </summary>
public class ListDocumentsRequest
{
    public Guid? UserId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

/// <summary>
/// FastEndpoint for listing documents with pagination
/// </summary>
public class ListDocumentsEndpoint : Endpoint<ListDocumentsRequest, ListDocumentsResult>
{
    private readonly IMediator _mediator;

    public ListDocumentsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/v1/documents");
        AllowAnonymous(); // TODO: Add authentication
        Description(d => d
            .WithName("ListDocuments")
            .WithTags("Documents")
            .Produces<ListDocumentsResult>(200)
            .ProducesProblemDetails(400));
    }

    public override async Task HandleAsync(ListDocumentsRequest req, CancellationToken ct)
    {
        var query = new ListDocumentsQuery
        {
            UserId = req.UserId,
            Status = req.Status,
            Page = req.Page,
            PageSize = req.PageSize,
            SortBy = req.SortBy,
            SortOrder = req.SortOrder
        };

        var result = await _mediator.Send(query, ct);

        await SendOkAsync(result, ct);
    }
}
