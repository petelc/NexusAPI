using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.UseCases.Documents.Commands;
using Nexus.API.UseCases.Documents.DTOs;
using Nexus.API.UseCases.Documents.Commands.UpdateDocument;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: PUT /api/v1/documents/{id}
/// Replaces title, content, and/or status of an existing document.
/// Only the document owner may update it (Phase B permissions will extend this).
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateDocumentEndpoint : Endpoint<UpdateDocumentRequest>
{
    private readonly IMediator _mediator;

    public UpdateDocumentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/documents/{id}");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Update a document")
            .WithDescription("Updates the title, content, and/or status of a document. Updating content automatically creates a version snapshot."));
    }

    public override async Task HandleAsync(UpdateDocumentRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue("uid");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Unauthorized" }, ct);
            return;
        }

        var documentIdStr = Route<string>("id");
        if (!Guid.TryParse(documentIdStr, out var documentId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid document ID" }, ct);
            return;
        }

        try
        {
            var command = new UpdateDocumentCommand
            {
                DocumentId = new DocumentId(documentId),
                Title = req.Title,
                Content = req.Content,
                Status = req.Status,
                UpdatedBy = userId
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Document not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to update this document" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                HttpContext.Response.StatusCode = 422;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Validation failed" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to update document" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
