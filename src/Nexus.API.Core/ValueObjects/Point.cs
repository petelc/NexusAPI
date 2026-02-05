using Ardalis.GuardClauses;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Represents a 2D point (x, y coordinates) on the diagram canvas
/// </summary>
public class Point : ValueObject
{
  public double X { get; private set; }
  public double Y { get; private set; }

  private Point() { } // EF Core

  private Point(double x, double y)
  {
    X = x;
    Y = y;
  }

  public static Point Create(double x, double y)
  {
    return new Point(x, y);
  }

  public Point MoveTo(double newX, double newY)
  {
    return new Point(newX, newY);
  }

  public Point Offset(double dx, double dy)
  {
    return new Point(X + dx, Y + dy);
  }

  public double DistanceTo(Point other)
  {
    Guard.Against.Null(other, nameof(other));
    var dx = other.X - X;
    var dy = other.Y - Y;
    return Math.Sqrt(dx * dx + dy * dy);
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return X;
    yield return Y;
  }

  public override string ToString() => $"({X}, {Y})";
}
