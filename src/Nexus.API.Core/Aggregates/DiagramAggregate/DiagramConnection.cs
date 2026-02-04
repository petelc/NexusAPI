using Ardalis.GuardClauses;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.DiagramAggregate;

/// <summary>
/// Represents a connection (line/arrow) between two diagram elements
/// </summary>
public class DiagramConnection : EntityBase<ConnectionId>
{
  public ElementId SourceElementId { get; private set; }
  public ElementId TargetElementId { get; private set; }
  public ConnectionType ConnectionType { get; private set; }
  public ConnectionStyle Style { get; private set; } = null!;
  public string? Label { get; private set; }

  // Control points for curves, stored as JSON
  private string? _controlPoints;
  public string? ControlPoints
  {
    get => _controlPoints;
    private set => _controlPoints = value;
  }

  private DiagramConnection() { } // EF Core

  private DiagramConnection(
    ConnectionId id,
    ElementId sourceElementId,
    ElementId targetElementId,
    ConnectionType connectionType,
    ConnectionStyle style,
    string? label)
  {
    if (sourceElementId.Value == targetElementId.Value)
      throw new DomainException("Source and target elements cannot be the same");

    Id = id;
    SourceElementId = Guard.Against.Null(sourceElementId, nameof(sourceElementId));
    TargetElementId = Guard.Against.Null(targetElementId, nameof(targetElementId));
    ConnectionType = connectionType;
    Style = style ?? ConnectionStyle.CreateDefault();
    Label = label;
  }

  public static DiagramConnection Create(
    ElementId sourceElementId,
    ElementId targetElementId,
    ConnectionType connectionType = ConnectionType.Arrow,
    string? label = null,
    ConnectionStyle? style = null)
  {
    return new DiagramConnection(
      ConnectionId.CreateNew(),
      sourceElementId,
      targetElementId,
      connectionType,
      style ?? ConnectionStyle.CreateDefault(),
      label
    );
  }

  public void UpdateLabel(string? label)
  {
    Label = label;
  }

  public void UpdateStyle(ConnectionStyle newStyle)
  {
    Style = Guard.Against.Null(newStyle, nameof(newStyle));
  }

  public void UpdateConnectionType(ConnectionType type)
  {
    ConnectionType = type;
  }

  public void SetControlPoints(string? jsonControlPoints)
  {
    ControlPoints = jsonControlPoints;
  }
}
