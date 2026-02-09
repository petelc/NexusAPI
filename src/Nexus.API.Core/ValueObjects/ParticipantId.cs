namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for SessionParticipant entity
/// </summary>
public readonly struct ParticipantId : IEquatable<ParticipantId>
{
    public Guid Value { get; }

    private ParticipantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ParticipantId cannot be empty", nameof(value));

        Value = value;
    }

    public static ParticipantId Create(Guid value) => new(value);
    public static ParticipantId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ParticipantId id) => id.Value;
    public static explicit operator ParticipantId(Guid value) => new(value);

    public bool Equals(ParticipantId other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is ParticipantId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ParticipantId left, ParticipantId right) => left.Equals(right);

    public static bool operator !=(ParticipantId left, ParticipantId right) => !left.Equals(right);
}