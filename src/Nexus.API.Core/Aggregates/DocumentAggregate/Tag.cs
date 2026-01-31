using Ardalis.GuardClauses;
using Traxs.SharedKernel;

namespace Nexus.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Represents a tag that can be applied to documents, diagrams, and snippets
/// </summary>
public class Tag : EntityBase<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Color { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Private constructor for EF Core
    private Tag() { }
    
    /// <summary>
    /// Factory method to create a new tag
    /// </summary>
    public static Tag Create(string name, string? color = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 50, "Tag name must be between 1 and 50 characters");
        
        if (color != null)
        {
            Guard.Against.InvalidFormat(color, nameof(color), @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", "Color must be a valid hex code");
        }
        
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim().ToLowerInvariant(),
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
        
        return tag;
    }
    
    /// <summary>
    /// Update the tag color
    /// </summary>
    public void UpdateColor(string? color)
    {
        if (color != null)
        {
            Guard.Against.InvalidFormat(color, nameof(color), @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", "Color must be a valid hex code");
        }
        
        Color = color;
    }
}
