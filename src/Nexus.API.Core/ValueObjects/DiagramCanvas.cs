using Ardalis.GuardClauses;
using Nexus.API.Core.Exceptions;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Represents the canvas/workspace for a diagram
/// </summary>
public class DiagramCanvas : ValueObject
{
  public double Width { get; private set; }
  public double Height { get; private set; }
  public string BackgroundColor { get; private set; } = null!;
  public int? GridSize { get; private set; }

  private DiagramCanvas() { } // EF Core

  private DiagramCanvas(double width, double height, string backgroundColor, int? gridSize)
  {
    Width = width;
    Height = height;
    BackgroundColor = backgroundColor;
    GridSize = gridSize;
  }

  public static DiagramCanvas Create(double width, double height, string? backgroundColor = null, int? gridSize = null)
  {
    if (width <= 0 || width > 10000)
      throw new DomainException("Canvas width must be between 1 and 10000");
    if (height <= 0 || height > 10000)
      throw new DomainException("Canvas height must be between 1 and 10000");
    if (gridSize.HasValue && (gridSize.Value < 1 || gridSize.Value > 100))
      throw new DomainException("Grid size must be between 1 and 100");

    return new DiagramCanvas(
      width,
      height,
      backgroundColor ?? "#FFFFFF",
      gridSize ?? 20
    );
  }

  public static DiagramCanvas CreateDefault()
  {
    return Create(1920, 1080, "#FFFFFF", 20);
  }

  public bool IsWithinBounds(Point position, Size size)
  {
    Guard.Against.Null(position, nameof(position));
    Guard.Against.Null(size, nameof(size));

    return position.X >= 0 &&
           position.Y >= 0 &&
           position.X + size.Width <= Width &&
           position.Y + size.Height <= Height;
  }

  public DiagramCanvas Resize(double newWidth, double newHeight)
  {
    return Create(newWidth, newHeight, BackgroundColor, GridSize);
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Width;
    yield return Height;
    yield return BackgroundColor;
    yield return GridSize ?? 0;
  }
}
