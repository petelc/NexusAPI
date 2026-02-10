using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate.Events;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Team aggregate
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Get team by ID
    /// </summary>
    Task<Team?> GetByIdAsync(TeamId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get team by ID with members loaded
    /// </summary>
    Task<Team?> GetByIdWithMembersAsync(TeamId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all teams where user is a member
    /// </summary>
    Task<IEnumerable<Team>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search teams by name
    /// </summary>
    Task<IEnumerable<Team>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a team with the given name already exists
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, TeamId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new team
    /// </summary>
    Task AddAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing team
    /// </summary>
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a team (usually soft delete)
    /// </summary>
    Task DeleteAsync(Team team, CancellationToken cancellationToken = default);
}
