namespace Nexus.API.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Strongly-typed identifier for Document aggregate
/// </summary>
public readonly struct DocumentId : IEquatable<DocumentId>
{
    public Guid Value { get; }

    public DocumentId(Guid value)
    {
        Value = value;
    }

    public static DocumentId CreateNew() => new(Guid.NewGuid());

    public static DocumentId From(Guid value) => new(value);

    public static DocumentId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DocumentId id) => id.Value;

    public bool Equals(DocumentId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is DocumentId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(DocumentId left, DocumentId right) => left.Equals(right);

    public static bool operator !=(DocumentId left, DocumentId right) => !left.Equals(right);
}
