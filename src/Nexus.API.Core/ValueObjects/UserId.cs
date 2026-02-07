namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for User aggregate
/// </summary>
public readonly struct UserId : IEquatable<UserId>
{
  public Guid Value { get; }

  private UserId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("UserId cannot be empty", nameof(value));

    Value = value;
  }

  public static UserId Create(Guid value) => new(value);

  public static UserId From(Guid value) => new(value);

  public static UserId CreateNew() => new(Guid.NewGuid());

  public static UserId Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("UserId cannot be empty", nameof(value));

    if (!Guid.TryParse(value, out var guid))
      throw new ArgumentException("UserId must be a valid GUID", nameof(value));

    return new UserId(guid);
  }

  public override string ToString() => Value.ToString();

  public static implicit operator Guid(UserId userId) => userId.Value;

  public bool Equals(UserId other) => Value.Equals(other.Value);

  public override bool Equals(object? obj) => obj is UserId other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(UserId left, UserId right) => left.Equals(right);

  public static bool operator !=(UserId left, UserId right) => !left.Equals(right);
}