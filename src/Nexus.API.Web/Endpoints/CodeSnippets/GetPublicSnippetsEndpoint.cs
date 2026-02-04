using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.CodeSnippets.DTOs;

namespace Nexus.API.Web.Endpoints.CodeSnippets;

/// <summary>
/// Get public snippets endpoint
/// GET /api/v1/snippets/public
/// Supports pagination
/// </summary>
public class GetPublicSnippetsEndpoint : Endpoint<GetPublicSnippetsRequest, CodeSnippetPagedResultDto>
{
  private readonly ICodeSnippetRepository _snippetRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public GetPublicSnippetsEndpoint(
    ICodeSnippetRepository snippetRepository,
    UserManager<ApplicationUser> userManager)
  {
    _snippetRepository = snippetRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/snippets/public");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Code Snippets")
      .WithSummary("Get public snippets")
      .WithDescription("Retrieves public snippets with pagination support."));
  }

  public override async Task HandleAsync(
    GetPublicSnippetsRequest request,
    CancellationToken ct)
  {
    var page = request.Page ?? 1;
    var pageSize = request.PageSize ?? 20;

    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    // Get public snippets
    var snippets = await _snippetRepository.GetPublicSnippetsAsync(page, pageSize, ct);
    var snippetList = snippets.ToList();

    // Get total count
    var totalItems = await _snippetRepository.CountPublicSnippetsAsync(ct);
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

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
      LineCount = s.Metadata.LineCount,
      IsPublic = s.Metadata.IsPublic,
      ForkCount = s.Metadata.ForkCount,
      ViewCount = s.Metadata.ViewCount,
      CreatedBy = new UserInfoDto
      {
        UserId = s.CreatedBy,
        Username = users.TryGetValue(s.CreatedBy, out var username) ? username : "Unknown"
      },
      CreatedAt = s.CreatedAt,
      UpdatedAt = s.UpdatedAt,
      Tags = s.Tags.Select(t => t.Name).ToList()
    }).ToList();


    var response = new CodeSnippetPagedResultDto
    {
      Items = items,
      Page = page,
      PageSize = pageSize,
      TotalCount = totalItems,
      TotalPages = totalPages,
      HasNextPage = page < totalPages,
      HasPreviousPage = page > 1
    };

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}

public record GetPublicSnippetsRequest(int? Page, int? PageSize);
