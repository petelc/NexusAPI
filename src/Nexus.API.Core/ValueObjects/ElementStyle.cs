namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Style properties for a diagram element
/// </summary>
public class ElementStyle : ValueObject
{
  public string FillColor { get; private set; } = null!;
  public string StrokeColor { get; private set; } = null!;
  public int StrokeWidth { get; private set; }
  public int FontSize { get; private set; }
  public string FontFamily { get; private set; } = null!;
  public double Opacity { get; private set; }
  //public int ZIndex { get; private set; }
  public double Rotation { get; private set; }

  private ElementStyle() { } // EF Core

  private ElementStyle(
    string fillColor,
    string strokeColor,
    int strokeWidth,
    int fontSize,
    string fontFamily,
    double opacity,
    //int zIndex,
    double rotation)
  {
    FillColor = fillColor;
    StrokeColor = strokeColor;
    StrokeWidth = strokeWidth;
    FontSize = fontSize;
    FontFamily = fontFamily;
    Opacity = opacity;
    //ZIndex = zIndex;
    Rotation = rotation;
  }

  public static ElementStyle CreateDefault()
  {
    return new ElementStyle(
      fillColor: "#FFFFFF",
      strokeColor: "#000000",
      strokeWidth: 2,
      fontSize: 14,
      fontFamily: "Arial",
      opacity: 1.0,
      //zIndex: 0,
      rotation: 0.0
    );
  }

  public static ElementStyle Create(
    string? fillColor = null,
    string? strokeColor = null,
    int? strokeWidth = null,
    int? fontSize = null,
    string? fontFamily = null,
    double? opacity = null,
    //int? zIndex = null,
    double? rotation = null)
  {
    return new ElementStyle(
      fillColor: fillColor ?? "#FFFFFF",
      strokeColor: strokeColor ?? "#000000",
      strokeWidth: strokeWidth ?? 2,
      fontSize: fontSize ?? 14,
      fontFamily: fontFamily ?? "Arial",
      opacity: opacity ?? 1.0,
      //zIndex: zIndex ?? 0,
      rotation: rotation ?? 0.0
    );
  }

  public ElementStyle WithFillColor(string color) =>
    new ElementStyle(color, StrokeColor, StrokeWidth, FontSize, FontFamily, Opacity, Rotation);

  public ElementStyle WithStrokeColor(string color) =>
    new ElementStyle(FillColor, color, StrokeWidth, FontSize, FontFamily, Opacity, Rotation);
  public ElementStyle WithOpacity(double opacity) =>
    new ElementStyle(FillColor, StrokeColor, StrokeWidth, FontSize, FontFamily, opacity, Rotation);

  public ElementStyle WithRotation(double rotation) =>
    new ElementStyle(FillColor, StrokeColor, StrokeWidth, FontSize, FontFamily, Opacity, rotation);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return FillColor;
    yield return StrokeColor;
    yield return StrokeWidth;
    yield return FontSize;
    yield return FontFamily;
    yield return Opacity;
    //yield return ZIndex;
    yield return Rotation;
  }
}
