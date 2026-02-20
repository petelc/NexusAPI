using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Diagrams.DTOs;
using Nexus.API.Core.Enums;

namespace Nexus.API.Web.Endpoints.Diagrams;

/// <summary>
/// Endpoint: POST /api/v1/diagrams/{diagramId}/connections
/// Adds a connection between two elements
/// Requires: Editor, Admin roles
/// </summary>
public class AddConnectionEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public AddConnectionEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Post("/diagrams/{diagramId}/connections");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Connections")
      .WithSummary("Add connection to diagram")
      .WithDescription("Creates a connection between two elements in a diagram."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<AddConnectionRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
    if (request == null)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid request body" }, ct);
      return;
    }

    // Parse connection type
    if (!Enum.TryParse<ConnectionType>(request.ConnectionType, true, out var connectionType))
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid connection type" }, ct);
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

      // Create style
      var style = request.Style != null
        ? ConnectionStyle.Create(
            request.Style.StrokeColor,
            request.Style.StrokeWidth,
            request.Style.StrokeDashArray)
        : ConnectionStyle.CreateDefault();

      // Create connection
      var connection = DiagramConnection.Create(
        ElementId.Create(request.SourceElementId),
        ElementId.Create(request.TargetElementId),
        connectionType,
        request.Label,
        style);

      // Add to diagram
      diagram.AddConnection(connection);
      await _diagramRepository.UpdateAsync(diagram, ct);

      var response = new DiagramConnectionDto
      {
        ConnectionId = connection.Id.Value,
        SourceElementId = connection.SourceElementId.Value,
        TargetElementId = connection.TargetElementId.Value,
        ConnectionType = connection.ConnectionType.ToString(),
        Label = connection.Label,
        Style = new ConnectionStyleDto
        {
          StrokeColor = connection.Style.StrokeColor,
          StrokeWidth = connection.Style.StrokeWidth,
          StrokeDashArray = connection.Style.StrokeDashArray
        }
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
