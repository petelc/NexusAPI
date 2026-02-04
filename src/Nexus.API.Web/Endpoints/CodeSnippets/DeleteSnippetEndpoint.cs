using System.Security.Claims;
using FastEndpoints;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Delete code snippet endpoint
/// DELETE /api/v1/snippets/{id}
/// Soft delete - sets IsDeleted flag
/// </summary>
public class DeleteSnippetEndpoint : Endpoint<DeleteSnippetRequest>
{
  private readonly ICodeSnippetRepository _snippetRepository;

  public DeleteSnippetEndpoint(ICodeSnippetRepository snippetRepository)
  {
    _snippetRepository = snippetRepository;
  }

  public override void Configure()
  {
    Delete("/snippets/{id}");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Delete snippet")
      .WithDescription("Soft deletes a code snippet. Only the owner can delete."));
  }

  public override async Task HandleAsync(
    DeleteSnippetRequest request,
    CancellationToken ct)
  {
    var userId = User.FindFirstValue("uid");
    if (string.IsNullOrEmpty(userId))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "User not authenticated" }
      }, ct);
      return;
    }

    var userGuid = Guid.Parse(userId);

    var snippet = await _snippetRepository.GetByIdAsync(request.Id, ct);

    if (snippet == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Snippet not found" }
      }, ct);
      return;
    }

    // Check ownership
    if (!snippet.CanEdit(userGuid))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Only the owner can delete this snippet" }
      }, ct);
      return;
    }

    // Soft delete
    snippet.Delete();
    await _snippetRepository.UpdateAsync(snippet, ct);

    HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
  }
}

public record DeleteSnippetRequest(Guid Id);
