using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Infrastructure.Data;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;

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

    public async Task<Team?> GetByIdWithMembersAsync(TeamId id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Team>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Where(t => t.Members.Any(m => m.UserId == userId && m.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Team>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Team>();

        return await _context.Teams
            .Where(t => t.Name.Contains(searchTerm))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, TeamId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _context.Teams.Where(t => t.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Team team, CancellationToken cancellationToken = default)
    {
        // This will typically be a soft delete handled by the aggregate
        // But we still update the entity in the context
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
