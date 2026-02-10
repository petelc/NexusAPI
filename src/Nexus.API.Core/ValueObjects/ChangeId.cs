namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for SessionChange entity
/// </summary>
public readonly struct ChangeId : IEquatable<ChangeId>
{
    public Guid Value { get; }

    private ChangeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChangeId cannot be empty", nameof(value));

        Value = value;
    }

    public static ChangeId Create(Guid value) => new(value);
    public static ChangeId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ChangeId id) => id.Value;
    public static explicit operator ChangeId(Guid value) => new(value);

    public bool Equals(ChangeId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is ChangeId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ChangeId left, ChangeId right) => left.Equals(right);

    public static bool operator !=(ChangeId left, ChangeId right) => !left.Equals(right);
}