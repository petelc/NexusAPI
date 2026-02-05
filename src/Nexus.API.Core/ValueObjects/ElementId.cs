namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for DiagramElement entity
/// </summary>
public struct ElementId : IEquatable<ElementId>
{
  public Guid Value { get; private set; }

  private ElementId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("ElementId cannot be empty", nameof(value));
    Value = value;
  }

  public static ElementId Create(Guid value) => new ElementId(value);
  public static ElementId CreateNew() => new ElementId(Guid.NewGuid());

  public static implicit operator Guid(ElementId id) => id.Value;
  public static explicit operator ElementId(Guid value) => new ElementId(value);

  public bool Equals(ElementId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is ElementId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(ElementId left, ElementId right) => left.Equals(right);
  public static bool operator !=(ElementId left, ElementId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
