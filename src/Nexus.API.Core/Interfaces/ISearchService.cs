using Nexus.API.Core.Models;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Interface for search operations (Elasticsearch, Azure Search, etc.)
/// </summary>
public interface ISearchService
{
    Task InitializeIndexesAsync(CancellationToken cancellationToken = default);

    Task IndexDocumentAsync(
      Guid documentId,
      string title,
      string content,
      IEnumerable<string> tags,
      Guid userId,
      CancellationToken cancellationToken = default);

    Task<SearchResults> SearchDocumentsAsync(
      string query,
      int page = 1,
      int pageSize = 20,
      IEnumerable<string>? tags = null,
      Guid? userId = null,
      CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(
      Guid documentId,
      CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetSuggestionsAsync(
      string prefix,
      int maxSuggestions = 10,
      CancellationToken cancellationToken = default);
}