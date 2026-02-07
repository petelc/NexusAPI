namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Collection aggregate
/// </summary>
public struct CollectionId : IEquatable<CollectionId>
{
  public Guid Value { get; private set; }
  private CollectionId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("CollectionId cannot be empty", nameof(value));
    Value = value;
  }

  public static CollectionId Create(Guid value) => new CollectionId(value);
  public static CollectionId CreateNew() => new CollectionId(Guid.NewGuid());

  public static CollectionId From(string value) => new(Guid.Parse(value));

  // Equatable implementation
  public bool Equals(CollectionId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is CollectionId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static implicit operator Guid(CollectionId id) => id.Value;
  public static explicit operator CollectionId(Guid value) => new CollectionId(value);

  public static bool operator ==(CollectionId left, CollectionId right) => left.Equals(right);
  public static bool operator !=(CollectionId left, CollectionId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
