using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Team aggregate
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _context;

    public TeamRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Team?> GetByIdAsync(TeamId id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Team>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Where(t => t.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Team> AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);
        return team;
    }

    public async Task<Team> UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);
        return team;
    }

    Task ITeamRepository.UpdateAsync(Team team, CancellationToken cancellationToken)
    {
        return UpdateAsync(team, cancellationToken);
    }

    public Task DeleteAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Remove(team);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task<Team?> GetByIdWithMembersAsync(TeamId id, CancellationToken cancellationToken = default)
    {
        return _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<IEnumerable<Team>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return _context.Teams
            .Where(t => t.Name.Contains(searchTerm))
            .ToListAsync(cancellationToken)
            .ContinueWith(t => t.Result.AsEnumerable(), cancellationToken);
    }

}