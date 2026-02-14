using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: GET /api/v1/diagrams/my
/// Gets current user's diagrams with pagination
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetMyDiagramsEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public GetMyDiagramsEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/diagrams/my");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams")
      .WithSummary("Get my diagrams")
      .WithDescription("Retrieves the current user's diagrams with pagination."));
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

    // Parse query parameters
    var pageStr = HttpContext.Request.Query["page"].FirstOrDefault() ?? "1";
    var pageSizeStr = HttpContext.Request.Query["pageSize"].FirstOrDefault() ?? "20";

    if (!int.TryParse(pageStr, out var page) || page < 1)
    {
      page = 1;
    }

    if (!int.TryParse(pageSizeStr, out var pageSize) || pageSize < 1)
    {
      pageSize = 20;
    }

    pageSize = Math.Min(pageSize, 100); // Max 100 items per page

    try
    {
      // Call repository with correct parameters based on actual signature
      // Assuming GetPagedAsync(int page, int pageSize, Guid? createdBy, DiagramType? type, CancellationToken)
      var pagedResult = await _diagramRepository.GetPagedAsync(page, pageSize, userId, null, ct);

      // Get username for response
      var user = await _userManager.FindByIdAsync(userId.ToString());
      var username = user?.UserName ?? "Unknown";

      var totalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pageSize);
      var response = new DiagramPagedResultDto
      {
        Items = pagedResult.Items.Select(d => new DiagramListItemDto
        {
          DiagramId = d.Id.Value,
          Title = d.Title.Value,
          DiagramType = d.DiagramType.ToString(),
          ElementCount = d.Elements.Count,
          ConnectionCount = d.Connections.Count,
          CreatedBy = new UserInfoDto(userId, username),
          CreatedAt = d.CreatedAt,
          UpdatedAt = d.UpdatedAt
        }).ToList(),
        Page = page,
        PageSize = pageSize,
        TotalCount = pagedResult.TotalCount,
        TotalPages = totalPages,
        HasNextPage = page < totalPages,
        HasPreviousPage = page > 1
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
