using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Ardalis.Specification;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CodeSnippet aggregate
/// Provides data access for code snippets
/// </summary>
public class CodeSnippetRepository : ICodeSnippetRepository
{
  private readonly AppDbContext _dbContext;

  public CodeSnippetRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<CodeSnippet?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .FirstOrDefaultAsync(cs => cs.Id == id && !cs.IsDeleted, cancellationToken);
  }

  public async Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .Where(cs => cs.CreatedBy == userId && !cs.IsDeleted)
      .OrderByDescending(cs => cs.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<CodeSnippet>> GetPublicSnippetsAsync(
    int page = 1,
    int pageSize = 20,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .Where(cs => cs.Metadata.IsPublic && !cs.IsDeleted)
      .OrderByDescending(cs => cs.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<CodeSnippet>> GetByLanguageAsync(
    string language,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .Where(cs => cs.Language.Name == language && !cs.IsDeleted)
      .OrderByDescending(cs => cs.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<CodeSnippet>> GetByTagAsync(
    string tagName,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .Where(cs => cs.Tags.Any(t => t.Name == tagName.ToLowerInvariant()) && !cs.IsDeleted)
      .OrderByDescending(cs => cs.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<CodeSnippet>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
  {
    var lowerSearchTerm = searchTerm.ToLower();

    return await _dbContext.Set<CodeSnippet>()
      .Include(cs => cs.Tags)
      .Where(cs => !cs.IsDeleted &&
        (cs.Title.Value.ToLower().Contains(lowerSearchTerm) ||
         (cs.Description != null && cs.Description.ToLower().Contains(lowerSearchTerm)) ||
         cs.Code.ToLower().Contains(lowerSearchTerm)))
      .OrderByDescending(cs => cs.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .CountAsync(cs => cs.CreatedBy == userId && !cs.IsDeleted, cancellationToken);
  }

  public async Task<int> CountPublicSnippetsAsync(
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<CodeSnippet>()
      .CountAsync(cs => cs.Metadata.IsPublic && !cs.IsDeleted, cancellationToken);
  }

  public async Task<CodeSnippet> AddAsync(
    CodeSnippet entity,
    CancellationToken cancellationToken = default)
  {
    await _dbContext.Set<CodeSnippet>().AddAsync(entity, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return entity;
  }

  public async Task UpdateAsync(
    CodeSnippet entity,
    CancellationToken cancellationToken = default)
  {
    // When entity was loaded via GetByIdAsync, EF Core already tracks it.
    // Calling Update() on a tracked entity with OwnsMany (Forks) incorrectly
    // marks newly added SnippetFork entries as Modified instead of Added,
    // causing a DbUpdateConcurrencyException (0 rows affected) on INSERT.
    //
    // If the entity is already tracked (Unchanged/Modified), skip Update()
    // and rely on EF Core's snapshot-based change detection, which correctly
    // detects new owned collection entries and generates INSERT statements.
    if (_dbContext.Entry(entity).State == EntityState.Detached)
    {
      _dbContext.Set<CodeSnippet>().Update(entity);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(
    CodeSnippet entity,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<CodeSnippet>().Remove(entity);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public Task<List<CodeSnippet>> ListAsync(CancellationToken cancellationToken = default)
  {
    return _dbContext.Set<CodeSnippet>()
      .Where(cs => !cs.IsDeleted)
      .ToListAsync(cancellationToken);
  }
}
