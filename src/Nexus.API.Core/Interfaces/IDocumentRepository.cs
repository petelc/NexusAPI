using Nexus.API.Core.Aggregates.DocumentAggregate;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Document aggregate
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Get a document by its ID
    /// </summary>
    Task<Document?> GetByIdAsync(DocumentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents created by a specific user
    /// </summary>
    Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by collection ID
    /// </summary>
    Task<IEnumerable<Document>> GetByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search documents by text query
    /// </summary>
    Task<IEnumerable<Document>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents with a specific tag
    /// </summary>
    Task<IEnumerable<Document>> GetByTagAsync(string tagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a document to the repository
    /// </summary>
    new Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing document
    /// </summary>
    new Task<int> UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document
    /// </summary>
    new Task<int> DeleteAsync(Document document, CancellationToken cancellationToken = default);
}
