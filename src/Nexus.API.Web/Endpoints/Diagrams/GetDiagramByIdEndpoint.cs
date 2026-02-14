using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: GET /api/v1/diagrams/{diagramId}
/// Gets a diagram by ID with all elements, connections, and layers
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetDiagramByIdEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public GetDiagramByIdEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Get("/diagrams/{diagramId}");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams")
      .WithSummary("Get diagram by ID")
      .WithDescription("Retrieves a diagram with all its elements, connections, and layers."));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("diagramId"), out var diagramId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid diagram ID" }, ct);
      return;
    }

    try
    {
      var diagramIdVO = DiagramId.Create(diagramId);
      var diagram = await _diagramRepository.GetByIdAsync(diagramIdVO, ct);

      if (diagram == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Diagram not found" }, ct);
        return;
      }

      // Get username for response
      var user = await _userManager.FindByIdAsync(diagram.CreatedBy.ToString());

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
          GridSize = diagram.Canvas.GridSize ?? 20
        },
        Elements = diagram.Elements.Select(e => new DiagramElementDto
        {
          ElementId = e.Id.Value,
          ShapeType = e.ShapeType.ToString(),
          Position = new PointDto { X = (decimal)e.Position.X, Y = (decimal)e.Position.Y },
          Size = new SizeDto { Width = (decimal)e.Size.Width, Height = (decimal)e.Size.Height },
          Text = e.Text,
          Style = new ElementStyleDto
          {
            FillColor = e.Style.FillColor,
            StrokeColor = e.Style.StrokeColor,
            StrokeWidth = e.Style.StrokeWidth,
            FontSize = e.Style.FontSize,
            FontFamily = e.Style.FontFamily,
            Opacity = (decimal)e.Style.Opacity,
            Rotation = (decimal)e.Style.Rotation
          },
          LayerId = e.LayerId?.Value,
          ZIndex = e.ZIndex,
          IsLocked = e.IsLocked,
          CustomProperties = !string.IsNullOrEmpty(e.CustomProperties)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(e.CustomProperties)
            : null
        }).ToList(),
        Connections = diagram.Connections.Select(c => new DiagramConnectionDto
        {
          ConnectionId = c.Id.Value,
          SourceElementId = c.SourceElementId.Value,
          TargetElementId = c.TargetElementId.Value,
          ConnectionType = c.ConnectionType.ToString(),
          Label = c.Label,
          Style = new ConnectionStyleDto
          {
            StrokeColor = c.Style.StrokeColor,
            StrokeWidth = c.Style.StrokeWidth,
            StrokeDashArray = c.Style.StrokeDashArray
          }
        }).ToList(),
        Layers = diagram.Layers.Select(l => new LayerDto
        {
          LayerId = l.Id.Value,
          Name = l.Name,
          Order = l.Order,
          IsVisible = l.IsVisible,
          IsLocked = l.IsLocked
        }).ToList(),
        CreatedBy = new UserInfoDto(diagram.CreatedBy, user?.UserName ?? "Unknown"),
        CreatedAt = diagram.CreatedAt,
        UpdatedAt = diagram.UpdatedAt
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
