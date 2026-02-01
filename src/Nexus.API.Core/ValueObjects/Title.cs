using Ardalis.GuardClauses;
using Traxs.SharedKernel;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Value object representing a title with validation
/// </summary>
public class Title : ValueObject
{
    public string Value { get; private set; }

    private Title(string value)
    {
        Value = value;
    }

    public static Title Create(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value), "Title cannot be empty");
        Guard.Against.OutOfRange(value.Length, nameof(value), 1, 200, "Title must be between 1 and 200 characters");

        return new Title(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Title title) => title.Value;
}
