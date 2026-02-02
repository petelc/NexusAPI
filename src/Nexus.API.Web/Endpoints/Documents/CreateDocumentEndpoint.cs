using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Documents.Create;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Create a new document endpoint
/// POST /api/documents
/// </summary>
public class CreateDocumentEndpoint : Endpoint<CreateDocumentCommand>
{
  private readonly IMediator _mediator;

  public CreateDocumentEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/documents");
    AllowAnonymous(); // TODO: Add authentication

    Description(b => b
      .WithTags("Documents")
      .WithSummary("Create a new document")
      .WithDescription("Creates a new document with the specified title and content"));
  }

  public override async Task HandleAsync(
    CreateDocumentCommand request,
    CancellationToken ct)
  {
    var result = await _mediator.Send(request, ct);

    HttpContext.Response.StatusCode = StatusCodes.Status201Created;
    HttpContext.Response.Headers.Location = $"/api/documents/{result.DocumentId}";
    await HttpContext.Response.WriteAsJsonAsync(result, ct);
  }
}
