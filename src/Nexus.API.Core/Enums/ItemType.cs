namespace Nexus.API.Core.Enums;

/// <summary>
/// Type of item that can be stored in a collection
/// </summary>
public enum ItemType
{
  /// <summary>
  /// Document item
  /// </summary>
  Document = 0,

  /// <summary>
  /// Diagram item
  /// </summary>
  Diagram = 1,

  /// <summary>
  /// Code snippet item
  /// </summary>
  Snippet = 2,

  /// <summary>
  /// Sub-collection (nested collection)
  /// </summary>
  SubCollection = 3
}
