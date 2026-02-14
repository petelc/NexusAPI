using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Endpoint: PUT /api/v1/snippets/{id}
/// Updates an existing code snippet
/// Requires: Owner or Editor with permission
/// </summary>
public class UpdateSnippetEndpoint : EndpointWithoutRequest
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly ITagRepository _tagRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public UpdateSnippetEndpoint(
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
    Put("/snippets/{id}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Update a snippet")
      .WithDescription("Updates an existing code snippet. Requires Editor or Admin roles."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateSnippetRequest>(ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    var snippet = await _snippetRepository.GetByIdAsync(snippetId, ct);
    if (snippet == null)
    {
      HttpContext.Response.StatusCode = 404;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Snippet not found" }, ct);
      return;
    }

    // Check permission
    if (!snippet.CanEdit(userId))
    {
      HttpContext.Response.StatusCode = 403;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "You do not have permission to edit this snippet" }, ct);
      return;
    }

    try
    {
      // Update title if provided
      Title? title = null;
      if (!string.IsNullOrWhiteSpace(request.Title))
      {
        title = Title.Create(request.Title);
      }

      // Update snippet
      snippet.Update(
        title: title,
        code: request.Code,
        description: request.Description);

      // Handle tags if provided
      if (request.Tags != null)
      {
        // Clear existing tags
        snippet.ClearTags();

        // Add new tags
        foreach (var tagName in request.Tags)
        {
          var tag = await _tagRepository.GetOrCreateAsync(tagName, color: null, cancellationToken: ct);
          snippet.AddTag(tag);
        }
      }

      await _snippetRepository.UpdateAsync(snippet, ct);

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

      HttpContext.Response.StatusCode = 200;
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
