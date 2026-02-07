namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for WorkspaceMember entity
/// </summary>
public readonly struct WorkspaceMemberId : IEquatable<WorkspaceMemberId>
{
  public Guid Value { get; init; }

  private WorkspaceMemberId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("WorkspaceMember ID cannot be empty", nameof(value));

    Value = value;
  }

  public static WorkspaceMemberId Create(Guid value) => new(value);
  public static WorkspaceMemberId CreateNew() => new(Guid.NewGuid());
  public static WorkspaceMemberId From(string value) => new(Guid.Parse(value));
  public override string ToString() => Value.ToString();

  // Implicit conversions
  public static implicit operator Guid(WorkspaceMemberId id) => id.Value;

  public bool Equals(WorkspaceMemberId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is WorkspaceMemberId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();
  public static bool operator ==(WorkspaceMemberId left, WorkspaceMemberId right) => left.Equals(right);
  public static bool operator !=(WorkspaceMemberId left, WorkspaceMemberId right) => !left.Equals(right);
}
