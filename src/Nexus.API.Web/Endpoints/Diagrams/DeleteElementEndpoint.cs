using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: DELETE /api/v1/diagrams/{diagramId}/elements/{elementId}
/// Deletes an element from a diagram
/// Requires: Editor, Admin roles
/// </summary>
public class DeleteElementEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;

  public DeleteElementEndpoint(IDiagramRepository diagramRepository)
  {
    _diagramRepository = diagramRepository;
  }

  public override void Configure()
  {
    Delete("/diagrams/{diagramId}/elements/{elementId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Elements")
      .WithSummary("Delete element")
      .WithDescription("Deletes an element from a diagram. Cannot delete elements with connections."));
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
      diagram.RemoveElement(elementIdVO);
      
      await _diagramRepository.UpdateAsync(diagram, ct);

      HttpContext.Response.StatusCode = 204;
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
