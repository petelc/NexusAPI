using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Tag entity
/// </summary>
public interface ITagRepository
{
  // Get or create pattern
  Task<Tag> GetOrCreateAsync(string name, string? color = null, CancellationToken cancellationToken = default);

  Task<Tag> GetOrCreateByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

  Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default);

  Task<Tag?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

  Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);

  Task<IEnumerable<Tag>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default);

  // CRUD operations
  Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);

  Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);

  Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);

}
