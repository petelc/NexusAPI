using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Tag entity
/// Handles tag CRUD operations with get-or-create pattern
/// </summary>
public class TagRepository : ITagRepository
{
  private readonly AppDbContext _dbContext;

  public TagRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<Tag?> GetByIdAsync(
        TagId id,
        CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<Tag>()
        .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public async Task<Tag?> GetByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
  {
    var normalizedName = name.Trim().ToLowerInvariant();
    return await _dbContext.Set<Tag>()
      .FirstOrDefaultAsync(t => t.Name == normalizedName, cancellationToken);
  }

  public async Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
  {
    var normalised = names
        .Select(n => n.Trim().ToLowerInvariant())
        .Distinct()
        .ToList();

    return await _dbContext.Set<Tag>()
        .Where(t => normalised.Contains(t.Name))
        .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Tag>> GetAllAsync(
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<Tag>()
      .OrderBy(t => t.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Tag>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
  {
    var lowerSearchTerm = searchTerm.ToLower();
    return await _dbContext.Set<Tag>()
      .Where(t => t.Name.Contains(lowerSearchTerm))
      .OrderBy(t => t.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<Tag> AddAsync(
        Tag entity,
        CancellationToken cancellationToken = default)
  {
    _dbContext.Set<Tag>().Add(entity);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return entity;
  }

  public async Task UpdateAsync(
        Tag entity,
        CancellationToken cancellationToken = default)
  {
    _dbContext.Set<Tag>().Update(entity);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(
        Tag entity,
        CancellationToken cancellationToken = default)
  {
    _dbContext.Set<Tag>().Remove(entity);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<Tag> GetOrCreateByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
  {
    var normalised = name.Trim().ToLowerInvariant();

    var existing = await _dbContext.Set<Tag>()
        .FirstOrDefaultAsync(t => t.Name == normalised, cancellationToken);

    if (existing is not null)
      return existing;

    var newTag = Tag.Create(normalised);
    _dbContext.Set<Tag>().Add(newTag);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return newTag;
  }

  public async Task<Tag> GetOrCreateAsync(string name, string? color = null, CancellationToken cancellationToken = default)
  {
    var normalised = name.Trim().ToLowerInvariant();

    var existing = await _dbContext.Set<Tag>()
      .FirstOrDefaultAsync(t => t.Name == normalised, cancellationToken);

    if (existing is not null)
      return existing;

    var newTag = Tag.Create(normalised, color);
    _dbContext.Set<Tag>().Add(newTag);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return newTag;
  }




}
