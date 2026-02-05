using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for Collection aggregate
/// </summary>
public interface ICollectionRepository
{
  /// <summary>
  /// Gets a collection by its ID
  /// </summary>
  Task<Collection?> GetByIdAsync(CollectionId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets all root collections in a workspace (collections with no parent)
  /// </summary>
  Task<List<Collection>> GetRootCollectionsAsync(
    WorkspaceId workspaceId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets all child collections of a parent collection
  /// </summary>
  Task<List<Collection>> GetChildCollectionsAsync(
    CollectionId parentId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets all collections in a workspace
  /// </summary>
  Task<List<Collection>> GetByWorkspaceIdAsync(
    WorkspaceId workspaceId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the full hierarchy path for a collection (all ancestors)
  /// </summary>
  Task<List<Collection>> GetHierarchyAsync(
    CollectionId collectionId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if moving a collection would create a circular reference
  /// </summary>
  Task<bool> WouldCreateCircularReferenceAsync(
    CollectionId collectionId,
    CollectionId targetParentId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Searches collections by name within a workspace
  /// </summary>
  Task<List<Collection>> SearchAsync(
    WorkspaceId workspaceId,
    string searchTerm,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds a new collection
  /// </summary>
  Task<Collection> AddAsync(Collection collection, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing collection
  /// </summary>
  Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes a collection (soft delete)
  /// </summary>
  Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a collection name already exists in the same parent
  /// </summary>
  Task<bool> ExistsWithNameAsync(
    WorkspaceId workspaceId,
    string name,
    CollectionId? parentId = null,
    CollectionId? excludeId = null,
    CancellationToken cancellationToken = default);
}
