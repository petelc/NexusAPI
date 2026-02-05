namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Strong-typed identifier for Workspace aggregate
/// </summary>
public struct WorkspaceId : IEquatable<WorkspaceId>
{
  public Guid Value { get; private set; }
  private WorkspaceId(Guid value)
  {
    if (value == Guid.Empty)
      throw new ArgumentException("WorkspaceId cannot be empty", nameof(value));
    Value = value;
  }

  public static WorkspaceId Create(Guid value) => new WorkspaceId(value);
  public static WorkspaceId CreateNew() => new WorkspaceId(Guid.NewGuid());

  public bool Equals(WorkspaceId other) => Value.Equals(other.Value);
  public override bool Equals(object? obj) => obj is WorkspaceId other && Equals(other);
  public override int GetHashCode() => Value.GetHashCode();

  public static implicit operator Guid(WorkspaceId id) => id.Value;
  public static explicit operator WorkspaceId(Guid value) => new WorkspaceId(value);

  public static bool operator ==(WorkspaceId left, WorkspaceId right) => left.Equals(right);
  public static bool operator !=(WorkspaceId left, WorkspaceId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}
