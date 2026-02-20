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
/// Endpoint: PUT /api/v1/diagrams/{diagramId}/elements/{elementId}
/// Updates an element's properties
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateElementEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public UpdateElementEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Put("/diagrams/{diagramId}/elements/{elementId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Elements")
      .WithSummary("Update element")
      .WithDescription("Updates an element's position, size, text, style, or other properties."));
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

    if (!Guid.TryParse(Route<string>("elementId"), out var elementId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid element ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateElementRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
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

      var elementIdVO = ElementId.Create(elementId);
      var element = diagram.Elements.FirstOrDefault(e => e.Id == elementIdVO);

      if (element == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Element not found" }, ct);
        return;
      }

      // Update position and size if provided
      if (request.Position != null && request.Size != null)
      {
        var position = Point.Create((double)request.Position.X, (double)request.Position.Y);
        var size = Core.ValueObjects.Size.Create((double)request.Size.Width, (double)request.Size.Height);
        diagram.UpdateElement(elementIdVO, position, size);
      }
      else if (request.Position != null)
      {
        var position = Point.Create((double)request.Position.X, (double)request.Position.Y);
        diagram.UpdateElement(elementIdVO, position, element.Size);
      }
      else if (request.Size != null)
      {
        var size = Core.ValueObjects.Size.Create((double)request.Size.Width, (double)request.Size.Height);
        diagram.UpdateElement(elementIdVO, element.Position, size);
      }

      // Update text if provided
      if (request.Text != null)
      {
        element.UpdateText(request.Text);
      }

      // Update style if provided
      if (request.Style != null)
      {
        var style = ElementStyle.Create(
          request.Style.FillColor,
          request.Style.StrokeColor,
          request.Style.StrokeWidth,
          request.Style.FontSize,
          request.Style.FontFamily,
          (double?)request.Style.Opacity,
          (double?)request.Style.Rotation);
        element.UpdateStyle(style);
      }

      // Update Z-index if provided
      if (request.ZIndex.HasValue)
      {
        element.UpdateZIndex(request.ZIndex.Value);
      }

      // Update locked state if provided
      if (request.IsLocked.HasValue)
      {
        if (request.IsLocked.Value)
          element.Lock();
        else
          element.Unlock();
      }

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
