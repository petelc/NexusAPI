namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Style properties for diagram connections
/// </summary>
public class ConnectionStyle : ValueObject
{
  public string StrokeColor { get; private set; } = null!;
  public int StrokeWidth { get; private set; }
  public string? StrokeDashArray { get; private set; }

  private ConnectionStyle() { } // EF Core

  private ConnectionStyle(string strokeColor, int strokeWidth, string? strokeDashArray)
  {
    StrokeColor = strokeColor;
    StrokeWidth = strokeWidth;
    StrokeDashArray = strokeDashArray;
  }

  public static ConnectionStyle CreateDefault()
  {
    return new ConnectionStyle(
      strokeColor: "#000000",
      strokeWidth: 2,
      strokeDashArray: null
    );
  }

  public static ConnectionStyle Create(
    string? strokeColor = null,
    int? strokeWidth = null,
    string? strokeDashArray = null)
  {
    return new ConnectionStyle(
      strokeColor: strokeColor ?? "#000000",
      strokeWidth: strokeWidth ?? 2,
      strokeDashArray: strokeDashArray
    );
  }

  public ConnectionStyle WithColor(string color) =>
    new ConnectionStyle(color, StrokeWidth, StrokeDashArray);

  public ConnectionStyle WithWidth(int width) =>
    new ConnectionStyle(StrokeColor, width, StrokeDashArray);

  public ConnectionStyle WithDashArray(string? dashArray) =>
    new ConnectionStyle(StrokeColor, StrokeWidth, dashArray);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return StrokeColor;
    yield return StrokeWidth;
    yield return StrokeDashArray ?? string.Empty;
  }
}
