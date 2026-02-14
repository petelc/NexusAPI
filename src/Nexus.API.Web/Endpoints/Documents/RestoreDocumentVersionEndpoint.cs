using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Documents.Queries;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: POST /api/v1/documents/{id}/versions/{versionNumber}/restore
/// Restores the document's content to the state captured in the specified version.
/// The current content is first snapshotted as a new version before the restore applies.
/// Requires: Editor, Admin roles
/// </summary>
public class RestoreDocumentVersionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public RestoreDocumentVersionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/documents/{id}/versions/{versionNumber}/restore");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Restore a document to a previous version")
            .WithDescription("Restores the document's content to the state captured in the specified historical version. The current content is automatically snapshotted before the restore is applied."));
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

        var versionNumberStr = Route<string>("versionNumber");
        if (!int.TryParse(versionNumberStr, out var versionNumber) || versionNumber < 1)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "versionNumber must be a positive integer" }, ct);
            return;
        }

        try
        {
            var command = new RestoreDocumentVersionCommand(documentId, versionNumber, userId);
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to restore this document" }, ct);
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
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to restore version" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
