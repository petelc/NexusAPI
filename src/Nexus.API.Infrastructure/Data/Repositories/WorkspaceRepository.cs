using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Workspace aggregate
/// </summary>
public class WorkspaceRepository : IWorkspaceRepository
{
  private readonly AppDbContext _context;

  public WorkspaceRepository(AppDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task<Workspace?> GetByIdAsync(WorkspaceId id, CancellationToken cancellationToken = default)
  {
    return await _context.Workspaces
      .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
  }

  public async Task<Workspace?> GetByIdWithMembersAsync(WorkspaceId id, CancellationToken cancellationToken = default)
  {
    // Members are automatically loaded due to owned entity configuration
    return await _context.Workspaces
      .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
  }

  public async Task<IEnumerable<Workspace>> GetByTeamIdAsync(TeamId teamId, CancellationToken cancellationToken = default)
  {
    return await _context.Workspaces
      .Where(w => w.TeamId == teamId && !w.IsDeleted)
      .OrderBy(w => w.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Workspace>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
  {
    // Query workspaces where user is an active member
    var workspaces = await _context.Workspaces
      .Where(w => !w.IsDeleted)
      .ToListAsync(cancellationToken);

    return workspaces
      .Where(w => w.Members.Any(m => m.UserId == userId && m.IsActive))
      .OrderBy(w => w.Name);
  }

  public async Task<bool> ExistsByNameAndTeamAsync(string name, TeamId teamId, CancellationToken cancellationToken = default)
  {
    return await _context.Workspaces
      .AnyAsync(w => w.Name == name && w.TeamId == teamId && !w.IsDeleted, cancellationToken);
  }

  public async Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
  {
    await _context.Workspaces.AddAsync(workspace, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
    return workspace;
  }

  public async Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
  {
    _context.Workspaces.Update(workspace);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(Workspace workspace, CancellationToken cancellationToken = default)
  {
    // Soft delete - just update the entity
    _context.Workspaces.Update(workspace);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<IEnumerable<Workspace>> SearchByNameAsync(
    string searchTerm,
    TeamId? teamId = null,
    CancellationToken cancellationToken = default)
  {
    var query = _context.Workspaces
      .Where(w => !w.IsDeleted);

    if (teamId != null)
    {
      query = query.Where(w => w.TeamId == teamId);
    }

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      query = query.Where(w => w.Name.Contains(searchTerm) ||
                              (w.Description != null && w.Description.Contains(searchTerm)));
    }

    return await query
      .OrderBy(w => w.Name)
      .ToListAsync(cancellationToken);
  }
}
