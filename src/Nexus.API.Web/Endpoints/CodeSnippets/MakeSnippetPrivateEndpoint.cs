using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Make snippet private endpoint
/// POST /api/v1/snippets/{id}/unpublish
/// Cannot unpublish if already forked
/// </summary>
public class MakeSnippetPrivateEndpoint : Endpoint<MakeSnippetPrivateRequest, CodeSnippetResponseDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public MakeSnippetPrivateEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/snippets/{id}/unpublish");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Make snippet private")
      .WithDescription("Makes a public snippet private. Cannot unpublish if already forked. Only the owner can unpublish."));
  }

  public override async Task HandleAsync(
    MakeSnippetPrivateRequest request,
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
        error = new { message = "Only the owner can unpublish this snippet" }
      }, ct);
      return;
    }

    // Try to make private (will throw if forked)
    try
    {
      snippet.MakePrivate();
      await _snippetRepository.UpdateAsync(snippet, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = ex.Message }
      }, ct);
      return;
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


public record MakeSnippetPrivateRequest(Guid Id);
