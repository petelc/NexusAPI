using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Get code snippet by ID endpoint
/// GET /api/v1/snippets/{id}
/// Increments view count if not owner
/// </summary>
public class GetSnippetByIdEndpoint : Endpoint<GetSnippetByIdRequest, CodeSnippetResponseDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public GetSnippetByIdEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/snippets/{id}");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Get snippet by ID")
      .WithDescription("Retrieves a snippet by ID. Accessible if public or owned by user. Increments view count for non-owners."));
  }

  public override async Task HandleAsync(
    GetSnippetByIdRequest request,
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

    // Check view permissions
    if (!snippet.CanView(userGuid))
    {
      HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "You don't have permission to view this snippet" }
      }, ct);
      return;
    }

    // Increment view count if not the owner
    if (snippet.CreatedBy != userGuid)
    {
      snippet.IncrementViewCount();
      await _snippetRepository.UpdateAsync(snippet, ct);
    }

    // Get creator username
    var creator = await _userManager.FindByIdAsync(snippet.CreatedBy.ToString());

    var response = new CodeSnippetDto
    {
      SnippetId = snippet.Id,
      Title = snippet.Title.Value,
      Code = snippet.Code,
      Language = snippet.Language.Name,
      LanguageVersion = snippet.Language.Version,
      Description = snippet.Description,
      LineCount = snippet.Metadata.LineCount,
      CharacterCount = snippet.Metadata.CharacterCount,
      IsPublic = snippet.Metadata.IsPublic,
      ForkCount = snippet.Metadata.ForkCount,
      ViewCount = snippet.Metadata.ViewCount,
      OriginalSnippetId = snippet.OriginalSnippetId,
      CreatedBy = new UserInfoDto
      {
        UserId = snippet.CreatedBy,
        Username = creator?.UserName ?? "Unknown"
      },
      CreatedAt = snippet.CreatedAt,
      UpdatedAt = snippet.UpdatedAt,
      Tags = snippet.Tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
    };

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}

public record GetSnippetByIdRequest(Guid Id);
