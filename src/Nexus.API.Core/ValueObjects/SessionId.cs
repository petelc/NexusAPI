namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for CollaborationSession aggregate
/// </summary>
public readonly struct SessionId : IEquatable<SessionId>
{
    public Guid Value { get; }

    private SessionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SessionId cannot be empty", nameof(value));

        Value = value;
    }

    public static SessionId Create(Guid value) => new(value);
    public static SessionId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SessionId id) => id.Value;
    public static explicit operator SessionId(Guid value) => new(value);

    public bool Equals(SessionId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is SessionId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(SessionId left, SessionId right) => left.Equals(right);

    public static bool operator !=(SessionId left, SessionId right) => !left.Equals(right);
}
