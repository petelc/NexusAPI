namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Layer entity
/// </summary>
public struct LayerId : IEquatable<LayerId>
{
  public Guid Value { get; private set; }


  private LayerId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("LayerId cannot be empty", nameof(value));
    Value = value;
  }

  public static LayerId Create(Guid value) => new LayerId(value);
  public static LayerId CreateNew() => new LayerId(Guid.NewGuid());

  public static implicit operator Guid(LayerId id) => id.Value;
  public static explicit operator LayerId(Guid value) => new LayerId(value);

  public bool Equals(LayerId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is LayerId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(LayerId left, LayerId right) => left.Equals(right);
  public static bool operator !=(LayerId left, LayerId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
