namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for CollectionItem entity
/// </summary>
public struct CollectionItemId : IEquatable<CollectionItemId>
{
  public Guid Value { get; private set; }
  private CollectionItemId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("CollectionItemId cannot be empty", nameof(value));
    Value = value;
  }

  public static CollectionItemId Create(Guid value) => new CollectionItemId(value);
  public static CollectionItemId CreateNew() => new CollectionItemId(Guid.NewGuid());

  public bool Equals(CollectionItemId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is CollectionItemId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static implicit operator Guid(CollectionItemId id) => id.Value;
  public static explicit operator CollectionItemId(Guid value) => new CollectionItemId(value);

  public static bool operator ==(CollectionItemId left, CollectionItemId right) => left.Equals(right);
  public static bool operator !=(CollectionItemId left, CollectionItemId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
