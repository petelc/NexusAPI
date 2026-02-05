using Ardalis.GuardClauses;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.DiagramAggregate;

/// <summary>
/// Represents a visual element (shape) on the diagram
/// </summary>
public class DiagramElement : EntityBase<ElementId>
{
  public ShapeType ShapeType { get; private set; }
  public Point Position { get; private set; } = null!;
  public Size Size { get; private set; } = null!;
  public ElementStyle Style { get; private set; } = null!;
  public string? Text { get; private set; }
  public LayerId? LayerId { get; private set; }
  public bool IsLocked { get; private set; }
  public int ZIndex { get; private set; }

  // Custom properties stored as JSON
  private string? _customProperties;
  public string? CustomProperties
  {
    get => _customProperties;
    private set => _customProperties = value;
  }

  private DiagramElement() { } // EF Core

  private DiagramElement(
    ElementId id,
    ShapeType shapeType,
    Point position,
    Size size,
    ElementStyle style,
    string? text,
    LayerId? layerId,
    int zIndex = 0)
  {
    Id = id;
    ShapeType = shapeType;
    Position = Guard.Against.Null(position, nameof(position));
    Size = Guard.Against.Null(size, nameof(size));
    Style = style ?? ElementStyle.CreateDefault();
    Text = text;
    LayerId = layerId;
    ZIndex = zIndex;
    IsLocked = false;
  }

  public static DiagramElement Create(
    ShapeType shapeType,
    Point position,
    Size size,
    string? text = null,
    ElementStyle? style = null,
    LayerId? layerId = null)
  {
    return new DiagramElement(
      ElementId.CreateNew(),
      shapeType,
      position,
      size,
      style ?? ElementStyle.CreateDefault(),
      text,
      layerId
    );
  }

  public void UpdatePosition(Point newPosition)
  {
    if (IsLocked)
      throw new DomainException("Cannot move locked element");

    Position = Guard.Against.Null(newPosition, nameof(newPosition));
  }

  public void UpdateSize(Size newSize)
  {
    if (IsLocked)
      throw new DomainException("Cannot resize locked element");

    Size = Guard.Against.Null(newSize, nameof(newSize));
  }

  public void UpdateText(string? text)
  {
    if (IsLocked)
      throw new DomainException("Cannot edit locked element");

    Text = text;
  }

  public void UpdateStyle(ElementStyle newStyle)
  {
    if (IsLocked)
      throw new DomainException("Cannot change style of locked element");

    Style = Guard.Against.Null(newStyle, nameof(newStyle));
  }

  public void UpdateZIndex(int zIndex)
  {
    ZIndex = zIndex;
  }

  public void MoveToLayer(LayerId? layerId)
  {
    LayerId = layerId;
  }

  public void Lock()
  {
    IsLocked = true;
  }

  public void Unlock()
  {
    IsLocked = false;
  }

  public void SetCustomProperties(string? jsonProperties)
  {
    if (IsLocked)
      throw new DomainException("Cannot modify locked element");

    CustomProperties = jsonProperties;
  }
}
