using FastEndpoints;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Search tags endpoint
/// GET /api/v1/tags/search
/// </summary>
public class SearchTagsEndpoint : Endpoint<SearchTagsRequest, List<TagDto>>
{
  private readonly ITagRepository _tagRepository;

  public SearchTagsEndpoint(ITagRepository tagRepository)
  {
    _tagRepository = tagRepository;
  }

  public override void Configure()
  {
    Get("/tags/search");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Tags")
      .WithSummary("Search tags")
      .WithDescription("Searches tags by keyword in tag name."));
  }

  public override async Task HandleAsync(
    SearchTagsRequest request,
    CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(request.Q))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Search query (q) is required" }
      }, ct);
      return;
    }

    var tags = await _tagRepository.SearchAsync(request.Q, ct);

    var response = tags
      .Select(t => new TagDto(t.Id, t.Name, t.Color))
      .ToList();

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}

public record SearchTagsRequest(string? Q);
