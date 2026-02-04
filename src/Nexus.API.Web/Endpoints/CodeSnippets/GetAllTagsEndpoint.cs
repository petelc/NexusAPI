using FastEndpoints;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Get all tags endpoint
/// GET /api/v1/tags
/// </summary>
public class GetAllTagsEndpoint : EndpointWithoutRequest<List<TagDto>>
{
  private readonly ITagRepository _tagRepository;

  public GetAllTagsEndpoint(ITagRepository tagRepository)
  {
    _tagRepository = tagRepository;
  }

  public override void Configure()
  {
    Get("/tags");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Tags")
      .WithSummary("Get all tags")
      .WithDescription("Retrieves all tags in the system."));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var tags = await _tagRepository.GetAllAsync(ct);

    var response = tags
      .Select(t => new TagDto(t.Id, t.Name, t.Color))
      .ToList();

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}
