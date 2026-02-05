namespace Nexus.API.Core.Enums;

/// <summary>
/// Types of diagrams supported by the system
/// </summary>
public enum DiagramType
{
  /// <summary>
  /// Flowchart diagram for process flows
  /// </summary>
  Flowchart = 0,

  /// <summary>
  /// Network diagram for infrastructure
  /// </summary>
  NetworkDiagram = 1,

  /// <summary>
  /// UML diagram for software design
  /// </summary>
  UmlDiagram = 2,

  /// <summary>
  /// Entity-Relationship diagram for databases
  /// </summary>
  ErDiagram = 3,

  /// <summary>
  /// Custom diagram type
  /// </summary>
  Custom = 99
}
