using Ardalis.GuardClauses;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.Aggregates.WorkspaceAggregate.Events;

namespace Nexus.API.Core.Aggregates.WorkspaceAggregate;

/// <summary>
/// Workspace aggregate root
/// A team's collaborative space containing collections and members
/// </summary>
public class Workspace : EntityBase<WorkspaceId>, IAggregateRoot
{
  public string Name { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public TeamId TeamId { get; private set; }
  public UserId CreatedBy { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public bool IsDeleted { get; private set; }
  public DateTime? DeletedAt { get; private set; }

  private readonly List<WorkspaceMember> _members = new();
  public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

  // EF Core constructor
  private Workspace() { }

  private Workspace(
    WorkspaceId id,
    string name,
    string? description,
    TeamId teamId,
    UserId createdBy,
    DateTime createdAt)
  {
    Id = Guard.Against.Null(id);
    Name = Guard.Against.NullOrWhiteSpace(name);
    Description = description;
    TeamId = Guard.Against.Null(teamId);
    CreatedBy = Guard.Against.Null(createdBy);
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
    IsDeleted = false;
  }

  /// <summary>
  /// Factory method to create a new workspace
  /// </summary>
  public static Workspace Create(
    string name,
    string? description,
    TeamId teamId,
    UserId createdBy)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.Null(teamId, nameof(teamId));
    Guard.Against.Null(createdBy, nameof(createdBy));

    if (name.Length > 200)
      throw new DomainException("Workspace name cannot exceed 200 characters");

    if (description?.Length > 1000)
      throw new DomainException("Workspace description cannot exceed 1000 characters");

    var workspace = new Workspace(
      WorkspaceId.CreateNew(),
      name,
      description,
      teamId,
      createdBy,
      DateTime.UtcNow);

    // Add creator as owner
    workspace.AddMember(createdBy, WorkspaceMemberRole.Owner, null);

    workspace.RegisterDomainEvent(new WorkspaceCreatedEvent(workspace.Id, workspace.TeamId, createdBy));

    return workspace;
  }

  /// <summary>
  /// Updates workspace name and description
  /// </summary>
  public void Update(string? name, string? description)
  {
    if (!string.IsNullOrWhiteSpace(name))
    {
      if (name.Length > 200)
        throw new DomainException("Workspace name cannot exceed 200 characters");
      Name = name;
    }

    if (description != null)
    {
      if (description.Length > 1000)
        throw new DomainException("Workspace description cannot exceed 1000 characters");
      Description = description;
    }

    UpdatedAt = DateTime.UtcNow;
    RegisterDomainEvent(new WorkspaceUpdatedEvent(Id));
  }

  /// <summary>
  /// Adds a member to the workspace
  /// </summary>
  public void AddMember(UserId userId, WorkspaceMemberRole role, UserId? invitedBy)
  {
    Guard.Against.Null(userId, nameof(userId));

    if (_members.Any(m => m.UserId == userId && m.IsActive))
      throw new DomainException("User is already a member of this workspace");

    var member = WorkspaceMember.Create(userId, role, invitedBy);
    _members.Add(member);

    UpdatedAt = DateTime.UtcNow;
    RegisterDomainEvent(new WorkspaceMemberAddedEvent(Id, userId, role));
  }

  /// <summary>
  /// Removes a member from the workspace
  /// </summary>
  public void RemoveMember(UserId userId, UserId removedBy)
  {
    Guard.Against.Null(userId, nameof(userId));
    Guard.Against.Null(removedBy, nameof(removedBy));

    var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    if (member == null)
      throw new DomainException("Member not found in workspace");

    // Cannot remove the last owner
    if (member.IsOwner() && _members.Count(m => m.IsOwner() && m.IsActive) == 1)
      throw new DomainException("Cannot remove the last owner from the workspace");

    // Check if the person removing has permission
    var remover = _members.FirstOrDefault(m => m.UserId == removedBy && m.IsActive);
    if (remover == null || !remover.CanManageMembers())
      throw new DomainException("User does not have permission to remove members");

    member.Deactivate();
    UpdatedAt = DateTime.UtcNow;
    RegisterDomainEvent(new WorkspaceMemberRemovedEvent(Id, userId));
  }

  /// <summary>
  /// Changes a member's role
  /// </summary>
  public void ChangeMemberRole(UserId userId, WorkspaceMemberRole newRole, UserId changedBy)
  {
    Guard.Against.Null(userId, nameof(userId));
    Guard.Against.Null(changedBy, nameof(changedBy));

    var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    if (member == null)
      throw new DomainException("Member not found in workspace");

    // Check permissions
    var changer = _members.FirstOrDefault(m => m.UserId == changedBy && m.IsActive);
    if (changer == null || !changer.CanManageMembers())
      throw new DomainException("User does not have permission to change member roles");

    // Cannot demote the last owner
    if (member.IsOwner() && newRole != WorkspaceMemberRole.Owner &&
        _members.Count(m => m.IsOwner() && m.IsActive) == 1)
      throw new DomainException("Cannot demote the last owner");

    member.ChangeRole(newRole);
    UpdatedAt = DateTime.UtcNow;
    RegisterDomainEvent(new WorkspaceMemberRoleChangedEvent(Id, userId, newRole));
  }

  /// <summary>
  /// Soft deletes the workspace
  /// </summary>
  public void Delete()
  {
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    RegisterDomainEvent(new WorkspaceDeletedEvent(Id));
  }

  /// <summary>
  /// Checks if a user is a member of the workspace
  /// </summary>
  public bool IsMember(UserId userId)
  {
    return _members.Any(m => m.UserId == userId && m.IsActive);
  }

  /// <summary>
  /// Checks if a user can manage members
  /// </summary>
  public bool CanManageMembers(UserId userId)
  {
    var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    return member?.CanManageMembers() ?? false;
  }

  /// <summary>
  /// Checks if a user can edit content
  /// </summary>
  public bool CanEditContent(UserId userId)
  {
    var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    return member?.CanEditContent() ?? false;
  }

  /// <summary>
  /// Gets a member's role
  /// </summary>
  public WorkspaceMemberRole? GetMemberRole(UserId userId)
  {
    return _members.FirstOrDefault(m => m.UserId == userId && m.IsActive)?.Role;
  }
}
