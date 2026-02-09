namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for CollaborationSession aggregate
/// </summary>
public readonly struct CollaborationId : IEquatable<CollaborationId>
{
    public Guid Value { get; }

    private CollaborationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CollaborationId cannot be empty", nameof(value));

        Value = value;
    }

    public static CollaborationId Create(Guid value) => new(value);
    public static CollaborationId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CollaborationId id) => id.Value;
    public static explicit operator CollaborationId(Guid value) => new(value);

    public bool Equals(CollaborationId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is CollaborationId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(CollaborationId left, CollaborationId right) => left.Equals(right);

    public static bool operator !=(CollaborationId left, CollaborationId right) => !left.Equals(right);
}