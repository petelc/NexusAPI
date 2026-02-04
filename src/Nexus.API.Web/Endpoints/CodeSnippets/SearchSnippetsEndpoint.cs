using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Search snippets endpoint
/// GET /api/v1/snippets/search
/// Searches title, description, and code content
/// </summary>
public class SearchSnippetsEndpoint : Endpoint<SearchSnippetsRequest, CodeSnippetPagedResultDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public SearchSnippetsEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/snippets/search");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Search snippets")
      .WithDescription("Searches snippets by keyword in title, description, or code content. Only returns public snippets or user's own snippets."));
  }

  public override async Task HandleAsync(
    SearchSnippetsRequest request,
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

    // Search snippets
    var allSnippets = await _snippetRepository.SearchAsync(request.Q, ct);

    // Filter to only public or user's own snippets
    var snippetList = allSnippets
      .Where(s => s.Metadata.IsPublic || s.CreatedBy == userGuid)
      .ToList();

    // Get usernames
    var userIds = snippetList.Select(s => s.CreatedBy.ToString()).Distinct().ToList();
    var users = new Dictionary<Guid, string>();
    foreach (var uid in userIds)
    {
      var user = await _userManager.FindByIdAsync(uid);
      users[Guid.Parse(uid)] = user?.UserName ?? "Unknown";
    }

    // Map to list items
    var items = snippetList.Select(s => new CodeSnippetListItemDto
    {
      SnippetId = s.Id,
      Title = s.Title.Value,
      Language = s.Language.Name,
      Description = s.Description,
      IsPublic = s.Metadata.IsPublic,
      LineCount = s.Metadata.LineCount,
      ForkCount = s.Metadata.ForkCount,
      ViewCount = s.Metadata.ViewCount,
      CreatedBy = new UserInfoDto
      {
        UserId = s.CreatedBy,
        Username = users.GetValueOrDefault(s.CreatedBy, "Unknown")
      },
      CreatedAt = s.CreatedAt,
      Tags = s.Tags.Select(t => t.Name).ToList()
    }).ToList();

    var response = new CodeSnippetPagedResultDto
    {
      Items = items,
      Page = 1,
      PageSize = items.Count,
      TotalCount = items.Count,
      TotalPages = 1,
      HasNextPage = false,
      HasPreviousPage = false
    };

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}

public record SearchSnippetsRequest(string? Q);
