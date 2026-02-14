using Ardalis.GuardClauses;
using Traxs.SharedKernel;
using Nexus.API.Core.ValueObjects;


namespace Nexus.API.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Represents a tag that can be applied to documents, diagrams, and snippets
/// </summary>
public class Tag : EntityBase<TagId>
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
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var normalised = name.Trim().ToLowerInvariant();
        if (normalised.Length > 50)
            throw new ArgumentException("Tag name cannot exceed 50 characters.", nameof(name));

        return new Tag
        {
            Id = TagId.CreateNew(),
            Name = normalised,
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
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
