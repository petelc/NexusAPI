using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Diagram aggregate
/// </summary>
public class DiagramRepository : IDiagramRepository
{
  private readonly AppDbContext _context;

  public DiagramRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<Diagram?> GetByIdAsync(DiagramId id, CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Include("_elements")
      .Include("_connections")
      .Include("_layers")
      .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public async Task<Diagram> AddAsync(Diagram diagram, CancellationToken cancellationToken = default)
  {
    await _context.Diagrams.AddAsync(diagram, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
    return diagram;
  }

  public async Task UpdateAsync(Diagram diagram, CancellationToken cancellationToken = default)
  {
    // The diagram is always loaded from this same DbContext instance, so it is already
    // tracked in Unchanged state. Calling Update() on a tracked entity with OwnsOne
    // value objects (Canvas, Title) marks their shadow FK key properties as Modified,
    // which EF Core forbids. Rely on change tracking instead: new elements/connections
    // added to navigation properties are automatically detected as Added.
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(Diagram diagram, CancellationToken cancellationToken = default)
  {
    _context.Diagrams.Remove(diagram);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<List<Diagram>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Where(d => d.CreatedBy == userId)
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Diagram>> GetByTypeAsync(DiagramType type, CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Where(d => d.DiagramType == type)
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Diagram>> GetByUserIdAndTypeAsync(
    Guid userId,
    DiagramType type,
    CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Where(d => d.CreatedBy == userId && d.DiagramType == type)
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<(List<Diagram> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    Guid? userId = null,
    DiagramType? type = null,
    CancellationToken cancellationToken = default)
  {
    var query = _context.Diagrams.AsQueryable();

    // Apply filters
    if (userId.HasValue)
    {
      query = query.Where(d => d.CreatedBy == userId.Value);
    }

    if (type.HasValue)
    {
      query = query.Where(d => d.DiagramType == type.Value);
    }

    // Get total count
    var totalCount = await query.CountAsync(cancellationToken);

    // Get paged results
    var items = await query
      .OrderByDescending(d => d.UpdatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (items, totalCount);
  }

  public async Task<List<Diagram>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Where(d => EF.Functions.Like(d.Title.Value, $"%{searchTerm}%"))
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .Where(d => d.CreatedBy == userId)
      .CountAsync(cancellationToken);
  }

  public async Task<bool> ExistsAsync(DiagramId id, CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .AnyAsync(d => d.Id == id, cancellationToken);
  }

  public async Task<List<Diagram>> ListAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Diagrams
      .OrderByDescending(d => d.CreatedAt)
      .ToListAsync(cancellationToken);
  }
}
