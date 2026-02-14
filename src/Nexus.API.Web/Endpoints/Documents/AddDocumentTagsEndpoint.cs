using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Documents.Commands;
using Nexus.API.UseCases.Documents.DTOs;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: POST /api/v1/documents/{id}/tags
/// Adds one or more tags to a document.
/// Tags are created automatically if they don't already exist.
/// Requires: Editor, Admin roles
/// </summary>
public class AddDocumentTagsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public AddDocumentTagsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/documents/{id}/tags");
        Roles("Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Add tags to a document")
            .WithDescription("Adds one or more tags to a document. Tags are created automatically if they do not already exist. Tag names are normalised to lower-case."));
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

        var request = await HttpContext.Request.ReadFromJsonAsync<AddDocumentTagsRequest>(ct);
        if (request == null || request.Tags == null || request.Tags.Count == 0)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Request body must contain a non-empty 'tags' array" }, ct);
            return;
        }

        try
        {
            var command = new AddDocumentTagsCommand(documentId, userId, request.Tags);
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
                await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to tag this document" }, ct);
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
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to add tags" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
