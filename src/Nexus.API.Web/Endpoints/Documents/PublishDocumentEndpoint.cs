using FastEndpoints;
using MediatR;
using Nexus.UseCases.Common.DTOs;
using Nexus.UseCases.Documents.Commands.PublishDocument;

namespace Nexus.Web.Endpoints.Documents;

/// <summary>
/// Request model for publishing a document
/// </summary>
public class PublishDocumentRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// FastEndpoint for publishing a document
/// </summary>
public class PublishDocumentEndpoint : Endpoint<PublishDocumentRequest, DocumentDto>
{
    private readonly IMediator _mediator;

    public PublishDocumentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/v1/documents/{id}/publish");
        AllowAnonymous(); // TODO: Add authentication
        Description(d => d
            .WithName("PublishDocument")
            .WithTags("Documents")
            .Produces<DocumentDto>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(400));
    }

    public override async Task HandleAsync(PublishDocumentRequest req, CancellationToken ct)
    {
        // TODO: Get user ID from claims
        var userId = Guid.NewGuid();

        var command = new PublishDocumentCommand
        {
            DocumentId = req.Id,
            PublishedBy = userId
        };

        var result = await _mediator.Send(command, ct);

        await SendOkAsync(result, ct);
    }
}
