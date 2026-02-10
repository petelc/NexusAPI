using Nexus.API.Core.Aggregates.TeamAggregate.Events;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.TeamAggregate;

/// <summary>
/// Team aggregate root
/// Represents a group of users collaborating together
/// Teams own workspaces and can have multiple members with different roles
/// </summary>
public class Team : EntityBase<TeamId>, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    // EF Core constructor
    private Team() { }

    private Team(
        TeamId id,
        string name,
        string? description,
        Guid createdBy,
        DateTime createdAt)
    {
        Id = Guard.Against.Null(id);
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description;
        CreatedBy = Guard.Against.Null(createdBy);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        IsDeleted = false;
    }

    /// <summary>
    /// Factory method to create a new team
    /// Creator is automatically added as Owner
    /// </summary>
    public static Team Create(string name, string? description, Guid createdBy)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        if (createdBy == Guid.Empty)
            throw new DomainException("CreatedBy cannot be empty");

        if (name.Length > 200)
            throw new DomainException("Team name cannot exceed 200 characters");

        if (description?.Length > 1000)
            throw new DomainException("Team description cannot exceed 1000 characters");

        var team = new Team(
            TeamId.CreateNew(),
            name.Trim(),
            description?.Trim(),
            createdBy,
            DateTime.UtcNow);

        // Creator becomes the first owner
        var ownerMember = TeamMember.Create(team.Id, createdBy, TeamRole.Owner);
        team._members.Add(ownerMember);

        // Raise domain event
        team.RegisterDomainEvent(new TeamCreatedEvent(
            team.Id,
            team.Name,
            createdBy,
            DateTime.UtcNow));

        return team;
    }

    /// <summary>
    /// Update team details
    /// </summary>
    public void Update(string? name, string? description)
    {
        var oldName = Name;
        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            if (name.Length > 200)
                throw new DomainException("Team name cannot exceed 200 characters");

            Name = name.Trim();
            hasChanges = true;
        }

        if (description != Description)
        {
            if (description?.Length > 1000)
                throw new DomainException("Team description cannot exceed 1000 characters");

            Description = description?.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            UpdatedAt = DateTime.UtcNow;

            RegisterDomainEvent(new TeamUpdatedEvent(
                Id,
                oldName,
                Name,
                DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Add a new member to the team
    /// </summary>
    public void AddMember(Guid userId, TeamRole role, Guid invitedBy)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        if (invitedBy == Guid.Empty)
            throw new DomainException("InvitedBy cannot be empty");

        // Check if user is already a member
        if (_members.Any(m => m.UserId == userId && m.IsActive))
            throw new DomainException("User is already a member of this team");

        var member = TeamMember.Create(Id, userId, role, invitedBy);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;

        RegisterDomainEvent(new TeamMemberAddedEvent(
            Id,
            member.Id,
            userId,
            role,
            invitedBy,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Remove a member from the team
    /// Cannot remove the last owner
    /// </summary>
    public void RemoveMember(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member == null)
            throw new DomainException("Member not found or already inactive");

        // Cannot remove last owner
        if (member.IsOwner())
        {
            var activeOwners = _members.Count(m => m.IsOwner() && m.IsActive);
            if (activeOwners <= 1)
                throw new DomainException("Cannot remove the last owner from the team");
        }

        member.Deactivate();
        UpdatedAt = DateTime.UtcNow;

        RegisterDomainEvent(new TeamMemberRemovedEvent(
            Id,
            member.Id,
            userId,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Change a member's role
    /// Cannot demote the last owner
    /// </summary>
    public void ChangeMemberRole(Guid userId, TeamRole newRole)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        var member = _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member == null)
            throw new DomainException("Member not found or already inactive");

        var oldRole = member.Role;

        // Cannot demote last owner
        if (member.IsOwner() && newRole != TeamRole.Owner)
        {
            var activeOwners = _members.Count(m => m.IsOwner() && m.IsActive);
            if (activeOwners <= 1)
                throw new DomainException("Cannot demote the last owner");
        }

        member.ChangeRole(newRole);
        UpdatedAt = DateTime.UtcNow;

        RegisterDomainEvent(new TeamMemberRoleChangedEvent(
            Id,
            member.Id,
            userId,
            oldRole,
            newRole,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Soft delete the team
    /// </summary>
    public void Delete(Guid deletedBy)
    {
        if (deletedBy == Guid.Empty)
            throw new DomainException("DeletedBy cannot be empty");

        if (IsDeleted)
            throw new DomainException("Team is already deleted");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RegisterDomainEvent(new TeamDeletedEvent(
            Id,
            deletedBy,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Check if a user is a member of this team
    /// </summary>
    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId && m.IsActive);
    }

    /// <summary>
    /// Get a member by user ID
    /// </summary>
    public TeamMember? GetMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    }

    /// <summary>
    /// Get the member's role
    /// </summary>
    public TeamRole? GetMemberRole(Guid userId)
    {
        return GetMember(userId)?.Role;
    }

    /// <summary>
    /// Check if user can manage members (Admin or Owner)
    /// </summary>
    public bool CanManageMembers(Guid userId)
    {
        var member = GetMember(userId);
        return member?.CanManageMembers() ?? false;
    }
}
