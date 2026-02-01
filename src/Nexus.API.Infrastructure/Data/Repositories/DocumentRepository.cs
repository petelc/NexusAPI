using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository for Document aggregate with custom query methods
/// </summary>
public class DocumentRepository : RepositoryBase<Document>, IDocumentRepository
{
  public DocumentRepository(AppDbContext dbContext) : base(dbContext)
  {
  }

  public async Task<Document?> GetByIdAsync(
    DocumentId id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Documents
      .Include(d => d.Tags)
      .Include(d => d.Versions)
      .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public async Task<IEnumerable<Document>> GetByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Documents
      .Include(d => d.Tags)
      .Where(d => d.CreatedBy == userId)
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Document>> GetByCollectionIdAsync(
    Guid collectionId,
    CancellationToken cancellationToken = default)
  {
    // This would join with CollectionItems table when that's implemented
    return await _dbContext.Documents
      .Include(d => d.Tags)
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Document>> SearchAsync(
    string query,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Documents
      .Include(d => d.Tags)
      .Where(d => d.Title.Value.Contains(query) ||
                  d.Content.PlainText.Contains(query))
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Document>> GetByTagAsync(
    string tag,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Documents
      .Include(d => d.Tags)
      .Where(d => d.Tags.Any(t => t.Name == tag))
      .OrderByDescending(d => d.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

}
