using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: PUT /api/v1/diagrams/{diagramId}
/// Updates diagram properties (title, canvas)
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateDiagramEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public UpdateDiagramEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Put("/diagrams/{diagramId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams")
      .WithSummary("Update diagram")
      .WithDescription("Updates diagram title and/or canvas properties."));
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

    if (!Guid.TryParse(Route<string>("diagramId"), out var diagramId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid diagram ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateDiagramRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
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

      // Update title if provided
      if (!string.IsNullOrWhiteSpace(request.Title))
      {
        var title = Title.Create(request.Title);
        diagram.UpdateTitle(title);
      }

      // Update canvas if provided
      if (request.Canvas != null)
      {
        diagram.ResizeCanvas(
          (double)request.Canvas.Width,
          (double)request.Canvas.Height);
      }

      await _diagramRepository.UpdateAsync(diagram, ct);

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
