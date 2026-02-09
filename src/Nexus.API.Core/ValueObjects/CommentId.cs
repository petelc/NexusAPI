namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Comment entity
/// </summary>
public readonly struct CommentId : IEquatable<CommentId>
{
    public Guid Value { get; }

    private CommentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommentId cannot be empty", nameof(value));

        Value = value;
    }

    public static CommentId Create(Guid value) => new(value);
    public static CommentId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CommentId id) => id.Value;
    public static explicit operator CommentId(Guid value) => new(value);

    public bool Equals(CommentId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is CommentId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(CommentId left, CommentId right) => left.Equals(right);

    public static bool operator !=(CommentId left, CommentId right) => !left.Equals(right);
}