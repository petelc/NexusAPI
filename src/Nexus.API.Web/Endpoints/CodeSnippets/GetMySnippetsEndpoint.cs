using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Get current user's snippets endpoint
/// GET /api/v1/snippets/my
/// </summary>
public class GetMySnippetsEndpoint : EndpointWithoutRequest<CodeSnippetPagedResultDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public GetMySnippetsEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/snippets/my");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Get my snippets")
      .WithDescription("Retrieves all snippets created by the current user."));
  }

  public override async Task HandleAsync(CancellationToken ct)
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
    var user = await _userManager.FindByIdAsync(userId);

    // Get all user's snippets
    var snippets = await _snippetRepository.GetByUserIdAsync(userGuid, ct);
    var snippetList = snippets.ToList();

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
        Username = user?.UserName ?? "Unknown"
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
