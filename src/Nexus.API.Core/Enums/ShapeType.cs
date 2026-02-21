namespace Nexus.API.Core.Enums;

/// <summary>
/// Types of shapes available for diagram elements
/// </summary>
public enum ShapeType
{
  // General
  Rectangle = 0,
  Circle = 1,
  Diamond = 2,
  Triangle = 3,
  Ellipse = 4,
  Hexagon = 5,

  // UML
  /// <summary>Stick-figure actor (use-case diagrams)</summary>
  Actor = 6,
  /// <summary>Rectangle with folded corner (annotation note)</summary>
  Note = 7,

  // Network
  /// <summary>Cloud outline (internet / cloud service)</summary>
  Cloud = 8,
  /// <summary>Cylinder (database / storage)</summary>
  Database = 9,

  // ER
  /// <summary>Double-border rectangle (weak entity)</summary>
  WeakEntity = 10,

  Custom = 99
}
