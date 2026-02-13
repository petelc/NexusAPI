using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Endpoint: POST /api/v1/snippets/{id}/fork
/// Forks (copies) a public code snippet
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class ForkSnippetEndpoint : EndpointWithoutRequest
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public ForkSnippetEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/snippets/{id}/fork");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Fork a snippet")
      .WithDescription("Forks (copies) a public code snippet. Requires Viewer, Editor, or Admin roles."));
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

    if (!Guid.TryParse(HttpContext.Request.RouteValues["id"]?.ToString(), out var snippetId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid snippet ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<ForkSnippetRequest>(ct);
    if (request == null || string.IsNullOrWhiteSpace(request.Title))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Title is required for forked snippet" }, ct);
      return;
    }

    var originalSnippet = await _snippetRepository.GetByIdAsync(snippetId, ct);
    if (originalSnippet == null)
    {
      HttpContext.Response.StatusCode = 404;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Snippet not found" }, ct);
      return;
    }

    try
    {
      // Create title for forked snippet
      var newTitle = Title.Create(request.Title);

      // Fork the snippet (will throw if not public)
      var forkedSnippet = originalSnippet.Fork(userId, newTitle);

      // Save both snippets
      await _snippetRepository.UpdateAsync(originalSnippet, ct);
      await _snippetRepository.AddAsync(forkedSnippet, ct);

      // Get creator username
      var user = await _userManager.FindByIdAsync(userId.ToString());

      var response = new CodeSnippetDto
      {
        SnippetId = forkedSnippet.Id,
        Title = forkedSnippet.Title.Value,
        Code = forkedSnippet.Code,
        Language = forkedSnippet.Language.Name,
        LanguageVersion = forkedSnippet.Language.Version,
        Description = forkedSnippet.Description,
        LineCount = forkedSnippet.Metadata.LineCount,
        CharacterCount = forkedSnippet.Metadata.CharacterCount,
        IsPublic = forkedSnippet.Metadata.IsPublic,
        ForkCount = forkedSnippet.Metadata.ForkCount,
        ViewCount = forkedSnippet.Metadata.ViewCount,
        OriginalSnippetId = forkedSnippet.OriginalSnippetId,
        CreatedBy = new UserInfoDto
        {
          UserId = userId,
          Username = user?.UserName ?? "Unknown"
        },
        CreatedAt = forkedSnippet.CreatedAt,
        UpdatedAt = forkedSnippet.UpdatedAt,
        Tags = forkedSnippet.Tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
      };

      HttpContext.Response.StatusCode = 201;
      HttpContext.Response.Headers.Append("Location", $"/api/v1/snippets/{forkedSnippet.Id}");
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
