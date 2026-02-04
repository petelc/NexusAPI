namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for DiagramConnection entity
/// </summary>
public struct ConnectionId : IEquatable<ConnectionId>
{
  public Guid Value { get; private set; }


  private ConnectionId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("ConnectionId cannot be empty", nameof(value));
    Value = value;
  }

  public static ConnectionId Create(Guid value) => new ConnectionId(value);
  public static ConnectionId CreateNew() => new ConnectionId(Guid.NewGuid());

  public static implicit operator Guid(ConnectionId id) => id.Value;
  public static explicit operator ConnectionId(Guid value) => new ConnectionId(value);

  public bool Equals(ConnectionId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is ConnectionId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(ConnectionId left, ConnectionId right) => left.Equals(right);
  public static bool operator !=(ConnectionId left, ConnectionId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
