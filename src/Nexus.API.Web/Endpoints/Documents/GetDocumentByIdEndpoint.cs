using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Documents.Get;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Get document by ID endpoint
/// GET /api/documents/{id}
/// </summary>
public class GetDocumentByIdEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public GetDocumentByIdEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/documents/{id}");
    AllowAnonymous(); // TODO: Add authentication

    Description(b => b
      .WithTags("Documents")
      .WithSummary("Get a document by ID")
      .WithDescription("Retrieves a document by its unique identifier"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var id = Route<Guid>("id");

    var query = new GetDocumentByIdQuery
    {
      Id = id,
      IncludePermissions = true
    };

    var result = await _mediator.Send(query, ct);

    if (result == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
      await HttpContext.Response.WriteAsJsonAsync(new { Message = "Document not found" }, ct);
      return;
    }

    await HttpContext.Response.WriteAsJsonAsync(result, ct);
  }
}
