using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Workspace aggregate
/// </summary>
public interface IWorkspaceRepository
{
  /// <summary>
  /// Gets a workspace by ID
  /// </summary>
  Task<Workspace?> GetByIdAsync(WorkspaceId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets workspaces by team ID
  /// </summary>
  Task<IEnumerable<Workspace>> GetByTeamIdAsync(TeamId teamId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets workspaces where user is a member
  /// </summary>
  Task<IEnumerable<Workspace>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a workspace name exists for a team
  /// </summary>
  Task<bool> ExistsByNameAndTeamAsync(string name, TeamId teamId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds a new workspace
  /// </summary>
  Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing workspace
  /// </summary>
  Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes a workspace
  /// </summary>
  Task DeleteAsync(Workspace workspace, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets workspace with members included
  /// </summary>
  Task<Workspace?> GetByIdWithMembersAsync(WorkspaceId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Searches workspaces by name
  /// </summary>
  Task<IEnumerable<Workspace>> SearchByNameAsync(string searchTerm, TeamId? teamId = null, CancellationToken cancellationToken = default);
}
