using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Diagram aggregate
/// </summary>
public interface IDiagramRepository
{
  // Basic CRUD
  Task<Diagram?> GetByIdAsync(DiagramId id, CancellationToken cancellationToken = default);
  Task<Diagram> AddAsync(Diagram diagram, CancellationToken cancellationToken = default);
  Task UpdateAsync(Diagram diagram, CancellationToken cancellationToken = default);
  Task DeleteAsync(Diagram diagram, CancellationToken cancellationToken = default);

  // Query methods
  Task<List<Diagram>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<List<Diagram>> GetByTypeAsync(DiagramType type, CancellationToken cancellationToken = default);
  Task<List<Diagram>> GetByUserIdAndTypeAsync(Guid userId, DiagramType type, CancellationToken cancellationToken = default);

  // Pagination
  Task<(List<Diagram> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    Guid? userId = null,
    DiagramType? type = null,
    CancellationToken cancellationToken = default);

  // Search
  Task<List<Diagram>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

  // Count
  Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

  // Existence check
  Task<bool> ExistsAsync(DiagramId id, CancellationToken cancellationToken = default);

  // List all (for admin)
  Task<List<Diagram>> ListAsync(CancellationToken cancellationToken = default);
}
