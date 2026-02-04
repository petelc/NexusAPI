using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
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
    _dbContext = dbContext;
  }

  public async Task<Tag?> GetByIdAsync(
    Guid id,
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
    Tag tag,
    CancellationToken cancellationToken = default)
  {
    await _dbContext.Set<Tag>().AddAsync(tag, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return tag;
  }

  public async Task UpdateAsync(
    Tag tag,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<Tag>().Update(tag);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(
    Tag tag,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<Tag>().Remove(tag);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<Tag> GetOrCreateAsync(
    string name,
    string? color = null,
    CancellationToken cancellationToken = default)
  {
    var existingTag = await GetByNameAsync(name, cancellationToken);
    if (existingTag != null)
      return existingTag;

    var newTag = Tag.Create(name, color);
    return await AddAsync(newTag, cancellationToken);
  }
}
