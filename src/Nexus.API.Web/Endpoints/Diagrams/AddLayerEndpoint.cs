using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: POST /api/v1/diagrams/{diagramId}/layers
/// Adds a new layer to a diagram
/// Requires: Editor, Admin roles
/// </summary>
public class AddLayerEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public AddLayerEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/diagrams/{diagramId}/layers");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Layers")
      .WithSummary("Add layer to diagram")
      .WithDescription("Adds a new layer to an existing diagram."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<AddLayerRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    // Validate name
    if (string.IsNullOrWhiteSpace(request.Name))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Layer name is required" }, ct);
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

      // Add layer
      diagram.AddLayer(request.Name);
      await _diagramRepository.UpdateAsync(diagram, ct);

      // Get the newly created layer (last one)
      var layer = diagram.Layers.OrderByDescending(l => l.Order).First();

      var response = new LayerDto
      {
        LayerId = layer.Id.Value,
        Name = layer.Name,
        Order = layer.Order,
        IsVisible = layer.IsVisible,
        IsLocked = layer.IsLocked
      };

      HttpContext.Response.StatusCode = 201;
      HttpContext.Response.Headers.Append("Location", $"/api/v1/diagrams/{diagramId}/layers/{layer.Id.Value}");
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
