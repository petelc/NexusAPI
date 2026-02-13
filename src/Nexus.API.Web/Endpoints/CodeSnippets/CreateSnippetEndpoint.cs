using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Endpoint: POST /api/v1/snippets
/// Creates a new code snippet
/// Requires: Editor, Admin roles
/// </summary>
public class CreateSnippetEndpoint : EndpointWithoutRequest
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly ITagRepository _tagRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public CreateSnippetEndpoint(
    ICodeSnippetRepository snippetRepository,
    ITagRepository tagRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _tagRepository = tagRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/snippets");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Create a new snippet")
      .WithDescription("Creates a new code snippet. Requires Editor or Admin roles."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<CreateSnippetRequest>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    // Validate required fields
    if (string.IsNullOrWhiteSpace(request.Title) ||
        string.IsNullOrWhiteSpace(request.Code) ||
        string.IsNullOrWhiteSpace(request.Language))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Title, Code, and Language are required" }, ct);
      return;
    }

    try
    {
      // Create title value object
      var title = Title.Create(request.Title);

      // Create programming language value object
      var language = ProgrammingLanguage.Create(
        request.Language,
        GetFileExtension(request.Language),
        request.LanguageVersion);

      // Create snippet
      var snippet = CodeSnippet.Create(title, request.Code, language, userId);

      // Add description if provided
      if (!string.IsNullOrWhiteSpace(request.Description))
      {
        snippet.Update(description: request.Description);
      }

      // Process tags if provided
      if (request.Tags != null && request.Tags.Any())
      {
        foreach (var tagName in request.Tags)
        {
          var tag = await _tagRepository.GetOrCreateAsync(tagName, color: null, cancellationToken: ct);
          snippet.AddTag(tag);
        }
      }

      // Save snippet
      await _snippetRepository.AddAsync(snippet, ct);

      // Get username for response
      var user = await _userManager.FindByIdAsync(userId.ToString());

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
        CreatedBy = new UserInfoDto
        {
          UserId = userId,
          Username = user?.UserName ?? "Unknown"
        },
        CreatedAt = snippet.CreatedAt,
        UpdatedAt = snippet.UpdatedAt,
        Tags = snippet.Tags.Select(t => new TagDto(t.Id, t.Name, t.Color)).ToList()
      };

      HttpContext.Response.StatusCode = 201;
      HttpContext.Response.Headers.Append("Location", $"/api/v1/snippets/{snippet.Id}");
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }

  private static string GetFileExtension(string language)
  {
    return language.ToLowerInvariant() switch
    {
      "csharp" or "c#" => ".cs",
      "javascript" => ".js",
      "typescript" => ".ts",
      "python" => ".py",
      "java" => ".java",
      "cpp" or "c++" => ".cpp",
      "c" => ".c",
      "ruby" => ".rb",
      "go" => ".go",
      "rust" => ".rs",
      "php" => ".php",
      "swift" => ".swift",
      "kotlin" => ".kt",
      "sql" => ".sql",
      "html" => ".html",
      "css" => ".css",
      "json" => ".json",
      "xml" => ".xml",
      "yaml" or "yml" => ".yaml",
      "markdown" or "md" => ".md",
      "bash" or "shell" => ".sh",
      _ => ".txt"
    };
  }
}
