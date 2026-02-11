namespace Nexus.API.Core.ValueObjects;

public struct TagId : IEquatable<TagId>
{
    public Guid Value { get; private set; }

    private TagId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TagId cannot be empty.", nameof(value));
        Value = value;
    }

    public static TagId Create(Guid value) => new TagId(value);

    public static TagId CreateNew() => new(Guid.NewGuid());

    public static implicit operator Guid(TagId id) => id.Value;
    public static explicit operator TagId(Guid value) => new TagId(value);

    public bool Equals(TagId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is TagId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(TagId left, TagId right) => left.Equals(right);
    public static bool operator !=(TagId left, TagId right) => !left.Equals(right);
}