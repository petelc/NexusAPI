namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// CollaborationId value object - unique identifier for a collaboration session
/// </summary>
public readonly struct ResourceId : IEquatable<ResourceId>
{
    public Guid Value { get; }

    private ResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ResourceId cannot be empty", nameof(value));

        Value = value;
    }

    public static ResourceId Create(Guid value) => new(value);
    public static ResourceId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ResourceId id) => id.Value;
    public static explicit operator ResourceId(Guid value) => new(value);

    public bool Equals(ResourceId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is ResourceId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ResourceId left, ResourceId right) => left.Equals(right);

    public static bool operator !=(ResourceId left, ResourceId right) => !left.Equals(right);
}