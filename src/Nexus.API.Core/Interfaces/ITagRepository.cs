using Nexus.API.Core.Aggregates.DocumentAggregate;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Tag entity
/// </summary>
public interface ITagRepository
{
  Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
  Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
  Task<IEnumerable<Tag>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

  // CRUD operations
  Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
  Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
  Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);

  // Get or create pattern
  Task<Tag> GetOrCreateAsync(string name, string? color = null, CancellationToken cancellationToken = default);
}
