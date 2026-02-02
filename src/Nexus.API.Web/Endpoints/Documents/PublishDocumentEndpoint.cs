using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Documents.Publish;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Publish document endpoint
/// POST /documents/{id}/publish
/// </summary>
public class PublishDocumentEndpoint : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public PublishDocumentEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/documents/{id}/publish");
    AllowAnonymous(); // TODO: Add authentication

    Description(b => b
      .WithTags("Documents")
      .WithSummary("Publish a document")
      .WithDescription("Changes document status from draft to published"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var id = Route<Guid>("id");

    var command = new PublishDocumentCommand
    {
      Id = id
    };

    try
    {
      var result = await _mediator.Send(command, ct);
      await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
    catch (InvalidOperationException ex)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
      await HttpContext.Response.WriteAsJsonAsync(new { Message = ex.Message }, ct);
    }
  }
}
