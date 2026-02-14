using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Documents.Commands;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: DELETE /api/v1/documents/{id}/tags/{tagName}
/// Removes a specific tag from a document.
/// The tag entity itself is NOT deleted — only the association is removed.
/// Requires: Editor, Admin roles
/// </summary>
public class RemoveDocumentTagEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public RemoveDocumentTagEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/documents/{id}/tags/{tagName}");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Remove a tag from a document")
            .WithDescription("Removes a tag from a document. The tag itself is not deleted — only the association between the document and the tag is removed."));
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

        var tagName = Route<string>("tagName");
        if (string.IsNullOrWhiteSpace(tagName))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "tagName route parameter is required" }, ct);
            return;
        }

        try
        {
            var command = new RemoveDocumentTagCommand(documentId, userId, tagName);
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                HttpContext.Response.StatusCode = 204;
                return;
            }
            else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Errors.FirstOrDefault() ?? "Not found" }, ct);
            }
            else if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                HttpContext.Response.StatusCode = 403;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to modify tags on this document" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to remove tag" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
