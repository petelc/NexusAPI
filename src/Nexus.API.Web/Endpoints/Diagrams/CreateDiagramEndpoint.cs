using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: POST /api/v1/diagrams
/// Creates a new diagram
/// Requires: Editor, Admin roles
/// </summary>
public class CreateDiagramEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public CreateDiagramEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/diagrams");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams")
      .WithSummary("Create a new diagram")
      .WithDescription("Creates a new diagram. Requires Editor or Admin roles."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<CreateDiagramRequest>(
      new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    // Validate required fields
    if (string.IsNullOrWhiteSpace(request.Title))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Title is required" }, ct);
      return;
    }

    // Parse diagram type
    if (!Enum.TryParse<DiagramType>(request.DiagramType, true, out var diagramType))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid diagram type" }, ct);
      return;
    }

    try
    {
      // Create title value object
      var title = Title.Create(request.Title);

      // Create canvas
      var canvas = request.Canvas != null
        ? DiagramCanvas.Create(
            (double)request.Canvas.Width,
            (double)request.Canvas.Height,
            request.Canvas.BackgroundColor,
            request.Canvas.GridSize)
        : DiagramCanvas.CreateDefault();

      // Create diagram
      var diagram = Diagram.Create(title, diagramType, userId, canvas);

      // Save diagram
      await _diagramRepository.AddAsync(diagram, ct);

      // Get username for response
      var user = await _userManager.FindByIdAsync(userId.ToString());

      var response = new DiagramDto
      {
        DiagramId = diagram.Id.Value,
        Title = diagram.Title.Value,
        DiagramType = diagram.DiagramType.ToString(),
        Canvas = new DiagramCanvasDto
        {
          Width = (decimal)diagram.Canvas.Width,
          Height = (decimal)diagram.Canvas.Height,
          BackgroundColor = diagram.Canvas.BackgroundColor,
          GridSize = diagram.Canvas.GridSize ?? 0
        },
        Elements = new List<DiagramElementDto>(),
        Connections = new List<DiagramConnectionDto>(),
        Layers = diagram.Layers.Select(l => new LayerDto
        {
          LayerId = l.Id.Value,
          Name = l.Name,
          Order = l.Order,
          IsVisible = l.IsVisible,
          IsLocked = l.IsLocked
        }).ToList(),
        CreatedBy = new UserInfoDto(userId, user?.UserName ?? "Unknown"),
        CreatedAt = diagram.CreatedAt,
        UpdatedAt = diagram.UpdatedAt
      };

      HttpContext.Response.StatusCode = 201;
      HttpContext.Response.Headers.Append("Location", $"/api/v1/diagrams/{diagram.Id.Value}");
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
