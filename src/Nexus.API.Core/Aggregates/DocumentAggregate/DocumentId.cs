namespace Nexus.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Strongly-typed identifier for Document aggregate
/// </summary>
public record DocumentId(Guid Value)
{
    public static DocumentId CreateNew() => new(Guid.NewGuid());
    
    public static DocumentId From(Guid value) => new(value);
    
    public static DocumentId From(string value) => new(Guid.Parse(value));
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(DocumentId id) => id.Value;
}
