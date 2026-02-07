using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.TeamAggregate;

/// <summary>
/// Team aggregate root
/// Represents a group of users collaborating together, owning workspaces and collections
/// </summary>
public class Team : EntityBase<TeamId>, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public UserId CreatedBy { get; private set; }
    public WorkspaceId? WorkspaceId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    private readonly List<Workspace> _workspaces = new();
    public IReadOnlyCollection<Workspace> Workspaces => _workspaces.AsReadOnly();

    private Team() { }

    private Team(TeamId id, string name, string? description, WorkspaceId? workspaceId, UserId createdBy, DateTime createdAt)
    {
        Id = Guard.Against.Null(id);
        Name = Guard.Against.NullOrWhiteSpace(name);
        Description = description;
        WorkspaceId = workspaceId;
        CreatedBy = Guard.Against.Null(createdBy);
        CreatedAt = createdAt;
    }

    public static Team Create(string name, string? description, WorkspaceId? workspaceId, UserId createdBy)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(createdBy, nameof(createdBy));
        Guard.Against.Null(workspaceId, nameof(WorkspaceId));
        Guard.Against.Null(createdBy, nameof(createdBy));

        if (name.Length > 200)
            throw new DomainException("Workspace name cannot exceed 200 characters");

        if (description?.Length > 1000)
            throw new DomainException("Workspace description cannot exceed 1000 characters");

        var team = new Team(TeamId.CreateNew(), name, description, workspaceId, createdBy, DateTime.UtcNow);

        team._members.Add(TeamMember.Create(team.Id, createdBy, TeamRole.Owner));

        return team;
    }
}
