namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Workspace aggregate
/// </summary>
public readonly struct WorkspaceId : IEquatable<WorkspaceId>
{
  public Guid Value { get; init; }

  private WorkspaceId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("Workspace ID cannot be empty", nameof(value));

    Value = value;
  }

  public static WorkspaceId Create(Guid value) => new(value);
  public static WorkspaceId CreateNew() => new(Guid.NewGuid());
  public static WorkspaceId From(string value) => new(Guid.Parse(value));

  public override string ToString() => Value.ToString();

  // Implicit conversions for convenience
  public static implicit operator Guid(WorkspaceId id) => id.Value;

  public bool Equals(WorkspaceId other) => Value.Equals(other.Value);

  public override bool Equals(object? obj) => obj is WorkspaceId other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode();

  public static bool operator ==(WorkspaceId left, WorkspaceId right) => left.Equals(right);

  public static bool operator !=(WorkspaceId left, WorkspaceId right) => !left.Equals(right);
}
