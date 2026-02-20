using FastEndpoints;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Diagrams.DTOs;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: PUT /diagrams/{diagramId}/layers/{layerId}
/// Updates a layer's properties
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateLayerEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;

  public UpdateLayerEndpoint(IDiagramRepository diagramRepository)
  {
    _diagramRepository = diagramRepository;
  }

  public override void Configure()
  {
    Put("/diagrams/{diagramId}/layers/{layerId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Layers")
      .WithSummary("Update layer")
      .WithDescription("Updates a layer's name, order, visibility, or lock state."));
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

    if (!Guid.TryParse(Route<string>("layerId"), out var layerId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid layer ID" }, ct);
      return;
    }

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateLayerRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
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

      var layerIdVO = LayerId.Create(layerId);
      var layer = diagram.Layers.FirstOrDefault(l => l.Id == layerIdVO);

      if (layer == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Layer not found" }, ct);
        return;
      }

      // Update name if provided
      if (!string.IsNullOrWhiteSpace(request.Name))
      {
        layer.Rename(request.Name);
      }

      // Update order if provided
      if (request.Order.HasValue)
      {
        layer.UpdateOrder(request.Order.Value);
      }

      // Update visibility if provided
      if (request.IsVisible.HasValue)
      {
        if (request.IsVisible.Value)
          layer.Show();
        else
          layer.Hide();
      }

      // Update locked state if provided
      if (request.IsLocked.HasValue)
      {
        if (request.IsLocked.Value)
          layer.Lock();
        else
          layer.Unlock();
      }

      await _diagramRepository.UpdateAsync(diagram, ct);

      var response = new LayerDto
      {
        LayerId = layer.Id.Value,
        Name = layer.Name,
        Order = layer.Order,
        IsVisible = layer.IsVisible,
        IsLocked = layer.IsLocked
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
