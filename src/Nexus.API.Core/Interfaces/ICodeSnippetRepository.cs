using Nexus.API.Core.Aggregates.CodeSnippetAggregate;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for CodeSnippet aggregate
/// </summary>
public interface ICodeSnippetRepository
{
  Task<CodeSnippet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<IEnumerable<CodeSnippet>> GetPublicSnippetsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
  Task<IEnumerable<CodeSnippet>> GetByLanguageAsync(string language, CancellationToken cancellationToken = default);
  Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tagName, CancellationToken cancellationToken = default);
  Task<IEnumerable<CodeSnippet>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
  Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<int> CountPublicSnippetsAsync(CancellationToken cancellationToken = default);

  // CRUD operations
  Task<CodeSnippet> AddAsync(CodeSnippet entity, CancellationToken cancellationToken = default);
  Task UpdateAsync(CodeSnippet entity, CancellationToken cancellationToken = default);
  Task DeleteAsync(CodeSnippet entity, CancellationToken cancellationToken = default);
  Task<List<CodeSnippet>> ListAsync(CancellationToken cancellationToken = default);
}
