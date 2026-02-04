namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Diagram aggregate
/// </summary>
/// [ValueObject<Guid>]
public struct DiagramId : IEquatable<DiagramId>
{
  public Guid Value { get; private set; }

  private DiagramId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("DiagramId cannot be empty", nameof(value));
    Value = value;
  }

  public static DiagramId Create(Guid value) => new DiagramId(value);
  public static DiagramId CreateNew() => new DiagramId(Guid.NewGuid());

  public static implicit operator Guid(DiagramId id) => id.Value;
  public static explicit operator DiagramId(Guid value) => new DiagramId(value);

  public bool Equals(DiagramId other) => Value.Equals(other.Value);

  public override bool Equals(object? obj) => obj is DiagramId other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(DiagramId left, DiagramId right) => left.Equals(right);

  public static bool operator !=(DiagramId left, DiagramId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
