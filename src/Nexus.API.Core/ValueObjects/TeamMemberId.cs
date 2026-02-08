namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for TeamMember entity
/// </summary>
public struct TeamMemberId : IEquatable<TeamMemberId>
{
    public Guid Value { get; private set; }

    private TeamMemberId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TeamMemberId cannot be empty", nameof(value));
        Value = value;
    }

    public static TeamMemberId Create(Guid value) => new TeamMemberId(value);
    public static TeamMemberId CreateNew() => new TeamMemberId(Guid.NewGuid());

    public static implicit operator Guid(TeamMemberId id) => id.Value;
    public static explicit operator TeamMemberId(Guid value) => new TeamMemberId(value);

    public bool Equals(TeamMemberId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is TeamMemberId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(TeamMemberId left, TeamMemberId right) => left.Equals(right);
    public static bool operator !=(TeamMemberId left, TeamMemberId right) => !left.Equals(right);
}
