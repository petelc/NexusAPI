using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Team aggregate
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets a team by ID
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team or null if not found</returns>
    Task<Team?> GetByIdAsync(TeamId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets teams by user ID (teams where the user is a member)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of teams</returns>
    Task<IEnumerable<Team>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new team
    /// </summary>
    /// <param name="team">Team to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added team</returns>
    Task<Team> AddAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing team
    /// </summary>
    /// <param name="team">Team to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a team
    /// </summary>
    /// <param name="team">Team to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team with members included
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team with members or null if not found</returns>
    Task<Team?> GetByIdWithMembersAsync(TeamId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches teams by name
    /// </summary>
    /// <param name="searchTerm">Search term for team name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of teams matching the search term</returns>
    Task<IEnumerable<Team>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}