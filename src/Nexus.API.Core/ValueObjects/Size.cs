using Nexus.API.Core.Exceptions;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Represents the dimensions (width and height) of a diagram element
/// </summary>
public class Size : ValueObject
{
  public double Width { get; private set; }
  public double Height { get; private set; }

  private Size() { } // EF Core

  private Size(double width, double height)
  {
    Width = width;
    Height = height;
  }

  public static Size Create(double width, double height)
  {
    if (width <= 0)
      throw new DomainException("Width must be positive");
    if (height <= 0)
      throw new DomainException("Height must be positive");

    return new Size(width, height);
  }

  public Size Resize(double newWidth, double newHeight)
  {
    return Create(newWidth, newHeight);
  }

  public double Area => Width * Height;
  public double AspectRatio => Width / Height;

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Width;
    yield return Height;
  }

  public override string ToString() => $"{Width} x {Height}";
}
