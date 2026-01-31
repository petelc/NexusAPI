using FastEndpoints;
using MediatR;
using Nexus.UseCases.Common.DTOs;
using Nexus.UseCases.Documents.Queries.GetDocumentById;

namespace Nexus.Web.Endpoints.Documents;

/// <summary>
/// Request model for getting a document by ID
/// </summary>
public class GetDocumentByIdRequest
{
    public Guid Id { get; set; }
    public bool IncludeVersions { get; set; } = false;
}

/// <summary>
/// FastEndpoint for getting a document by ID
/// </summary>
public class GetDocumentByIdEndpoint : Endpoint<GetDocumentByIdRequest, DocumentDto>
{
    private readonly IMediator _mediator;

    public GetDocumentByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/v1/documents/{id}");
        AllowAnonymous(); // TODO: Add authentication
        Description(d => d
            .WithName("GetDocumentById")
            .WithTags("Documents")
            .Produces<DocumentDto>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(GetDocumentByIdRequest req, CancellationToken ct)
    {
        var query = new GetDocumentByIdQuery
        {
            DocumentId = req.Id,
            IncludeVersions = req.IncludeVersions
        };

        var result = await _mediator.Send(query, ct);

        if (result == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(result, ct);
    }
}
