using FastEndpoints;
using System.Security.Claims;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: DELETE /api/v1/diagrams/{diagramId}/connections/{connectionId}
/// Deletes a connection from a diagram
/// Requires: Editor, Admin roles
/// </summary>
public class DeleteConnectionEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;

  public DeleteConnectionEndpoint(IDiagramRepository diagramRepository)
  {
    _diagramRepository = diagramRepository;
  }

  public override void Configure()
  {
    Delete("/diagrams/{diagramId}/connections/{connectionId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Connections")
      .WithSummary("Delete connection")
      .WithDescription("Deletes a connection from a diagram."));
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

    if (!Guid.TryParse(Route<string>("connectionId"), out var connectionId))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid connection ID" }, ct);
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

      var connectionIdVO = ConnectionId.Create(connectionId);
      diagram.RemoveConnection(connectionIdVO);
      
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
