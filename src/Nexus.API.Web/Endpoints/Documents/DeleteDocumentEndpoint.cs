using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Documents.Commands;
using Nexus.API.UseCases.Documents.Commands.DeleteDocument;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: DELETE /api/v1/documents/{id}
/// Soft-deletes a document by default; pass ?permanent=true for a hard delete.
/// Only the document owner may delete it.
/// Requires: Editor, Admin roles
/// </summary>
public class DeleteDocumentEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeleteDocumentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/documents/{id}");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Delete a document")
            .WithDescription("Soft-deletes a document (default). Pass ?permanent=true to permanently remove it. Only the document owner may delete."));
    }

    public override async Task HandleAsync(CancellationToken ct)
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

        var permanentStr = HttpContext.Request.Query["permanent"].ToString();
        var permanent = permanentStr.Equals("true", StringComparison.OrdinalIgnoreCase);

        try
        {
            var command = new DeleteDocumentCommand
            {
                DocumentId = documentId,
                DeletedBy = userId,
                Permanent = permanent
            };
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 204;
                return;
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Document not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to delete this document" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to delete document" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
