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
/// Endpoint: PUT /api/v1/diagrams/{diagramId}/connections/{connectionId}
/// Updates a connection's properties
/// Requires: Editor, Admin roles
/// </summary>
public class UpdateConnectionEndpoint : EndpointWithoutRequest
{
  private readonly IDiagramRepository _diagramRepository;
  private readonly UserManager<ApplicationUser> _userManager;

  public UpdateConnectionEndpoint(
    IDiagramRepository diagramRepository,
    UserManager<ApplicationUser> userManager)
  {
    _diagramRepository = diagramRepository;
    _userManager = userManager;
  }

  public override void Configure()
  {
    Put("/diagrams/{diagramId}/connections/{connectionId}");
    Roles("Editor", "Admin");

    Description(b => b
      .WithTags("Diagrams", "Connections")
      .WithSummary("Update connection")
      .WithDescription("Updates a connection's label and/or style."));
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

    var request = await HttpContext.Request.ReadFromJsonAsync<UpdateConnectionRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
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

      var connectionIdVO = ConnectionId.Create(connectionId);
      var connection = diagram.Connections.FirstOrDefault(c => c.Id == connectionIdVO);

      if (connection == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(new { error = "Connection not found" }, ct);
        return;
      }

      // Update label if provided
      if (request.Label != null)
      {
        connection.UpdateLabel(request.Label);
      }

      // Update style if provided
      if (request.Style != null)
      {
        var style = ConnectionStyle.Create(
          request.Style.StrokeColor,
          request.Style.StrokeWidth,
          request.Style.StrokeDashArray);
        connection.UpdateStyle(style);
      }

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
