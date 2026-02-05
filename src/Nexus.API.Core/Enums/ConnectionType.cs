namespace Nexus.API.Core.Enums;

/// <summary>
/// Types of connections between diagram elements
/// </summary>
public enum ConnectionType
{
  /// <summary>
  /// Simple straight line
  /// </summary>
  Line = 0,

  /// <summary>
  /// Line with arrow at target
  /// </summary>
  Arrow = 1,

  /// <summary>
  /// Line with arrows at both ends
  /// </summary>
  DoubleArrow = 2,

  /// <summary>
  /// Curved line (Bezier curve)
  /// </summary>
  BezierCurve = 3
}
