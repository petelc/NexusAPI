namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Team aggregate
/// </summary>
public struct TeamId : IEquatable<TeamId>
{
    public Guid Value { get; private set; }

    private TeamId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TeamId cannot be empty", nameof(value));
        Value = value;
    }

    public static TeamId Create(Guid value) => new TeamId(value);
    public static TeamId CreateNew() => new TeamId(Guid.NewGuid());

    public static implicit operator Guid(TeamId id) => id.Value;
    public static explicit operator TeamId(Guid value) => new TeamId(value);

    public bool Equals(TeamId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is TeamId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(TeamId left, TeamId right) => left.Equals(right);
    public static bool operator !=(TeamId left, TeamId right) => !left.Equals(right);
}
