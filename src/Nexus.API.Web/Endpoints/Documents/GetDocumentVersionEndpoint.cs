using MediatR;
using FastEndpoints;
using System.Security.Claims;
using Nexus.API.UseCases.Documents.Queries;

namespace Nexus.API.Web.Endpoints.Documents;

/// <summary>
/// Endpoint: GET /api/v1/documents/{id}/versions/{versionNumber}
/// Returns the full content of a specific historical version.
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetDocumentVersionEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetDocumentVersionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/documents/{id}/versions/{versionNumber}");
        Roles("Viewer", "Editor", "Admin");

        Description(b => b
            .WithTags("Documents")
            .WithSummary("Get a specific document version")
            .WithDescription("Returns the full rich-text content of a specific version snapshot."));
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
            var query = new GetDocumentVersionQuery(documentId, versionNumber, userId);
            var result = await _mediator.Send(query, ct);

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
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Insufficient permissions" }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(
                    new { error = result.Errors.FirstOrDefault() ?? "Failed to retrieve version" }, ct);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
    }
}
