using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.Enums;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: POST /api/v1/diagrams/{diagramId}/elements
/// Adds a new element to a diagram
/// Requires: Editor, Admin roles
/// </summary>
public class AddElementEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public AddElementEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/diagrams/{diagramId}/elements");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Elements")
      .WithSummary("Add element to diagram")
      .WithDescription("Adds a new element to an existing diagram."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<AddElementRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    // Parse shape type
    if (!Enum.TryParse<ShapeType>(request.ShapeType, true, out var shapeType))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid shape type" }, ct);
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

      // Create position, size, and style
      var position = Point.Create((double)request.Position.X, (double)request.Position.Y);
      var size = Core.ValueObjects.Size.Create((double)request.Size.Width, (double)request.Size.Height);

      var style = request.Style != null
        ? ElementStyle.Create(
            request.Style.FillColor,
            request.Style.StrokeColor,
            request.Style.StrokeWidth,
            request.Style.FontSize,
            request.Style.FontFamily,
            (double?)request.Style.Opacity,
            (double?)request.Style.Rotation)
        : ElementStyle.CreateDefault();

      // Validate layer if provided
      LayerId? layerId = null;
      if (request.LayerId.HasValue)
      {
        layerId = LayerId.Create(request.LayerId.Value);
        if (!diagram.Layers.Any(l => l.Id == layerId))
        {
          HttpContext.Response.StatusCode = 400;
          await HttpContext.Response.WriteAsJsonAsync(new { error = "Layer not found" }, ct);
          return;
        }
      }

      // Create element - Based on domain model, DiagramElement.Create takes:
      // (ShapeType, Point, Size, string?, ElementStyle, LayerId?, int)
      var element = DiagramElement.Create(
        shapeType,
        position,
        size,
        request.Text,
        style,
        layerId);

      // Add to diagram (ZIndex is auto-managed by domain)
      diagram.AddElement(element);
      await _diagramRepository.UpdateAsync(diagram, ct);

      var response = new DiagramElementDto
      {
        ElementId = element.Id.Value,
        ShapeType = element.ShapeType.ToString(),
        Position = new PointDto { X = (decimal)element.Position.X, Y = (decimal)element.Position.Y },
        Size = new SizeDto { Width = (decimal)element.Size.Width, Height = (decimal)element.Size.Height },
        Text = element.Text,
        Style = new ElementStyleDto
        {
          FillColor = element.Style.FillColor,
          StrokeColor = element.Style.StrokeColor,
          StrokeWidth = element.Style.StrokeWidth,
          FontSize = element.Style.FontSize,
          FontFamily = element.Style.FontFamily,
          Opacity = (decimal)element.Style.Opacity,
          Rotation = (decimal)element.Style.Rotation
        },
        LayerId = element.LayerId?.Value,
        ZIndex = element.ZIndex,
        IsLocked = element.IsLocked,
        CustomProperties = !string.IsNullOrEmpty(element.CustomProperties)
          ? JsonSerializer.Deserialize<Dictionary<string, object>>(element.CustomProperties)
          : null
      };

      HttpContext.Response.StatusCode = 201;
      HttpContext.Response.Headers.Append("Location", $"/api/v1/diagrams/{diagramId}");
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (InvalidOperationException ex)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }
}
