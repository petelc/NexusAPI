using FastEndpoints;
using MediatR;
using Nexus.UseCases.Common.DTOs;
using Nexus.UseCases.Documents.Commands.CreateDocument;

namespace Nexus.Web.Endpoints.Documents;

/// <summary>
/// Request model for creating a document
/// </summary>
public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? LanguageCode { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// FastEndpoint for creating a document
/// </summary>
public class CreateDocumentEndpoint : Endpoint<CreateDocumentRequest, DocumentDto>
{
    private readonly IMediator _mediator;

    public CreateDocumentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/v1/documents");
        AllowAnonymous(); // TODO: Add authentication with [Authorize]
        Description(d => d
            .WithName("CreateDocument")
            .WithTags("Documents")
            .Produces<DocumentDto>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(CreateDocumentRequest req, CancellationToken ct)
    {
        // TODO: Get user ID from claims after authentication is set up
        var userId = Guid.NewGuid(); // Temporary

        var command = new CreateDocumentCommand
        {
            Title = req.Title,
            Content = req.Content,
            CreatedBy = userId,
            LanguageCode = req.LanguageCode,
            Tags = req.Tags
        };

        var result = await _mediator.Send(command, ct);

        await SendCreatedAtAsync<GetDocumentByIdEndpoint>(
            new { id = result.Id },
            result,
            cancellation: ct);
    }
}
