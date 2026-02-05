using Nexus.API.Core.Exceptions;
using Traxs.SharedKernel;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.CollectionAggregate;

/// <summary>
/// Represents a hierarchical path for efficient tree queries
/// Format: /parent-id/child-id/grandchild-id/
/// </summary>
public sealed class HierarchyPath : ValueObject
{
  private const int MaxPathLength = 4000;
  private const int MaxLevel = 10;

  public string Value { get; private set; }
  public int Level { get; private set; }

  private HierarchyPath(string value, int level)
  {
    Value = value;
    Level = level;
  }

  /// <summary>
  /// Creates root-level hierarchy path
  /// </summary>
  public static HierarchyPath CreateRoot(CollectionId collectionId)
  {
    return new HierarchyPath($"/{collectionId.Value}/", 0);
  }

  /// <summary>
  /// Creates child hierarchy path from parent
  /// </summary>
  public static HierarchyPath CreateChild(HierarchyPath? parentPath, CollectionId collectionId)
  {
    if (parentPath is null)
    {
      return CreateRoot(collectionId);
    }

    if (parentPath.Level >= MaxLevel)
    {
      throw new DomainException($"Maximum hierarchy level of {MaxLevel} exceeded");
    }

    var newPath = $"{parentPath.Value}{collectionId.Value}/";

    if (newPath.Length > MaxPathLength)
    {
      throw new DomainException($"Hierarchy path exceeds maximum length of {MaxPathLength} characters");
    }

    return new HierarchyPath(newPath, parentPath.Level + 1);
  }

  /// <summary>
  /// Checks if this path is a descendant of the given path
  /// </summary>
  public bool IsDescendantOf(HierarchyPath other)
  {
    return Value.StartsWith(other.Value) && Value != other.Value;
  }

  /// <summary>
  /// Checks if this path is an ancestor of the given path
  /// </summary>
  public bool IsAncestorOf(HierarchyPath other)
  {
    return other.Value.StartsWith(Value) && Value != other.Value;
  }

  /// <summary>
  /// Gets the parent path (null if root)
  /// </summary>
  public HierarchyPath? GetParent()
  {
    if (Level == 0)
    {
      return null;
    }

    // Remove the last segment: /a/b/c/ -> /a/b/
    var lastSlash = Value.TrimEnd('/').LastIndexOf('/');
    if (lastSlash <= 0)
    {
      return null;
    }

    var parentPath = Value.Substring(0, lastSlash + 1);
    return new HierarchyPath(parentPath, Level - 1);
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}
