using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Make snippet public endpoint
/// POST /api/v1/snippets/{id}/publish
/// </summary>
public class MakeSnippetPublicEndpoint : Endpoint<MakeSnippetPublicRequest, CodeSnippetResponseDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public MakeSnippetPublicEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/snippets/{id}/publish");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Make snippet public")
      .WithDescription("Makes a private snippet public. Only the owner can publish."));
  }

  public override async Task HandleAsync(
    MakeSnippetPublicRequest request,
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
        error = new { message = "Only the owner can publish this snippet" }
      }, ct);
      return;
    }

    // Make public
    snippet.MakePublic();
    await _snippetRepository.UpdateAsync(snippet, ct);

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
      IsPublic = snippet.Metadata.IsPublic,
      LineCount = snippet.Metadata.LineCount,
      CharacterCount = snippet.Metadata.CharacterCount,
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

public record MakeSnippetPublicRequest(Guid Id);
