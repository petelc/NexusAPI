using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Nexus.API.Core.ValueObjects;
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
    // Disable AutoDetectChanges so that Entry() / Entries<T>() calls below do not
    // prematurely trigger DetectChanges (which would see replaced OwnsOne instances
    // as Delete+Add pairs and DELETE the CodeSnippets row).
    _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

    try
    {
      // Identify new forks (Detached = not yet in the DB) before touching the tracker.
      var newForks = entity.Forks
        .Where(f => _dbContext.Entry(f).State == EntityState.Detached)
        .ToList();

      var entityEntry = _dbContext.Entry(entity);

      if (entityEntry.State != EntityState.Detached)
      {
        // ── Tracked entity path (normal case: loaded via GetByIdAsync) ────────────
        //
        // We deliberately avoid two problematic operations:
        //
        // 1. entry.State = EntityState.Modified
        //    EF Core's raw state assignment propagates to every owned entity and marks
        //    their shadow FK key properties (e.g. ProgrammingLanguage.CodeSnippetId)
        //    as Modified → "property is part of a key" exception.
        //
        // 2. Update(entity) with AutoDetect enabled
        //    Triggers DetectChanges, which sees the replaced OwnsOne instance (e.g.
        //    Metadata after MakePublic()) as a Delete (old) + Add (new) pair, causing
        //    EF to DELETE the CodeSnippets row before a subsequent UPDATE finds 0 rows.
        //
        // Solution: use CurrentValues.SetValues() on each tracked owned-entity entry.
        // This copies new values into the EXISTING tracked entry in-place, marking only
        // the changed scalar properties as Modified — no state assignment, no DetectChanges.

        var metadataEntry = _dbContext.ChangeTracker.Entries<SnippetMetadata>().FirstOrDefault();
        if (metadataEntry != null)
          metadataEntry.CurrentValues.SetValues(entity.Metadata);

        var titleEntry = _dbContext.ChangeTracker.Entries<Title>().FirstOrDefault();
        if (titleEntry != null)
          titleEntry.CurrentValues.SetValues(entity.Title);

        // Sync CodeSnippet's own scalar properties (Code, Description, UpdatedAt, …).
        entityEntry.CurrentValues.SetValues(entity);
      }
      else
      {
        // ── Detached fallback ────────────────────────────────────────────────────
        // Entity was not loaded via a tracking query (uncommon). Use Update() to
        // re-attach. New forks are excluded from EF tracking (inserted via raw SQL
        // below) to avoid the OwnsMany FK-fixup / ProgrammingLanguage key error.
        _dbContext.Set<CodeSnippet>().Update(entity);

        // Update() marks existing forks as Modified (correct) and new forks as
        // Modified too (incorrect — new rows don't exist yet). Fix new forks.
        foreach (var fork in newForks)
        {
          var forkEntry = _dbContext.Entry(fork);
          if (forkEntry.State == EntityState.Modified)
            forkEntry.State = EntityState.Detached; // exclude; inserted via raw SQL below
        }
      }

      // ── Save CodeSnippet scalar + OwnsOne changes ──────────────────────────────
      await _dbContext.SaveChangesAsync(cancellationToken);

      // ── Insert new SnippetFork rows via raw SQL ─────────────────────────────────
      // We bypass EF's OwnsMany change-tracking for new forks because attaching a
      // new owned SnippetFork triggers FK fixup that tries to mark the shadow key
      // ProgrammingLanguage.CodeSnippetId as Modified → "part of a key" exception.
      foreach (var fork in newForks)
      {
        await _dbContext.Database.ExecuteSqlAsync(
          $"""
          INSERT INTO [dbo].[SnippetForks] ([OriginalSnippetId], [ForkedSnippetId], [ForkedBy], [ForkedAt])
          VALUES ({fork.OriginalSnippetId}, {fork.ForkedSnippetId}, {fork.ForkedBy}, {fork.ForkedAt})
          """,
          cancellationToken);
      }
    }
    finally
    {
      _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }
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
