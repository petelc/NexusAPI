using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Collection aggregate
/// </summary>
public class CollectionRepository : ICollectionRepository
{
  private readonly AppDbContext _context;

  public CollectionRepository(AppDbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public async Task<Collection?> GetByIdAsync(
    CollectionId id,
    CancellationToken cancellationToken = default)
  {
    return await _context.Collections
      .Include("_items")
      .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
  }

  public async Task<List<Collection>> GetRootCollectionsAsync(
    WorkspaceId workspaceId,
    CancellationToken cancellationToken = default)
  {
    return await _context.Collections
      .Include("_items")
      .Where(c => c.WorkspaceId == workspaceId && c.ParentCollectionId == null)
      .OrderBy(c => c.OrderIndex)
      .ThenBy(c => c.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Collection>> GetChildCollectionsAsync(
    CollectionId parentId,
    CancellationToken cancellationToken = default)
  {
    return await _context.Collections
      .Include("_items")
      .Where(c => c.ParentCollectionId == parentId)
      .OrderBy(c => c.OrderIndex)
      .ThenBy(c => c.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Collection>> GetByWorkspaceIdAsync(
    WorkspaceId workspaceId,
    CancellationToken cancellationToken = default)
  {
    return await _context.Collections
      .Include("_items")
      .Where(c => c.WorkspaceId == workspaceId)
      .OrderBy(c => c.HierarchyPath.Level)
      .ThenBy(c => c.OrderIndex)
      .ThenBy(c => c.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Collection>> GetHierarchyAsync(
    CollectionId collectionId,
    CancellationToken cancellationToken = default)
  {
    var collection = await GetByIdAsync(collectionId, cancellationToken);
    if (collection == null)
    {
      return new List<Collection>();
    }

    // Get all ancestors using hierarchy path
    // HierarchyPath format: /ancestor1-id/ancestor2-id/collection-id/
    var ancestorIds = ExtractAncestorIds(collection.HierarchyPath.Value);

    if (!ancestorIds.Any())
    {
      return new List<Collection> { collection };
    }

    var ancestors = await _context.Collections
      .Where(c => ancestorIds.Contains(c.Id.Value))
      .OrderBy(c => c.HierarchyPath.Level)
      .ToListAsync(cancellationToken);

    ancestors.Add(collection);
    return ancestors;
  }

  public async Task<bool> WouldCreateCircularReferenceAsync(
    CollectionId collectionId,
    CollectionId targetParentId,
    CancellationToken cancellationToken = default)
  {
    // Can't move to self
    if (collectionId == targetParentId)
    {
      return true;
    }

    var collection = await GetByIdAsync(collectionId, cancellationToken);
    var targetParent = await GetByIdAsync(targetParentId, cancellationToken);

    if (collection == null || targetParent == null)
    {
      return false;
    }

    // Check if target parent is a descendant of the collection being moved
    // If so, moving would create a cycle
    return targetParent.HierarchyPath.IsDescendantOf(collection.HierarchyPath);
  }

  public async Task<List<Collection>> SearchAsync(
    WorkspaceId workspaceId,
    string searchTerm,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      return await GetByWorkspaceIdAsync(workspaceId, cancellationToken);
    }

    var lowerSearch = searchTerm.ToLowerInvariant();

    return await _context.Collections
      .Include("_items")
      .Where(c => c.WorkspaceId == workspaceId &&
                  (c.Name.ToLowerInvariant().Contains(lowerSearch) ||
                   (c.Description != null && c.Description.ToLowerInvariant().Contains(lowerSearch))))
      .OrderBy(c => c.HierarchyPath.Level)
      .ThenBy(c => c.Name)
      .ToListAsync(cancellationToken);
  }

  public async Task<Collection> AddAsync(
    Collection collection,
    CancellationToken cancellationToken = default)
  {
    await _context.Collections.AddAsync(collection, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
    return collection;
  }

  public async Task UpdateAsync(
    Collection collection,
    CancellationToken cancellationToken = default)
  {
    _context.Collections.Update(collection);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(
    Collection collection,
    CancellationToken cancellationToken = default)
  {
    _context.Collections.Remove(collection);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<bool> ExistsWithNameAsync(
    WorkspaceId workspaceId,
    string name,
    CollectionId? parentId = null,
    CollectionId? excludeId = null,
    CancellationToken cancellationToken = default)
  {
    var query = _context.Collections
      .Where(c => c.WorkspaceId == workspaceId &&
                  c.Name.ToLowerInvariant() == name.ToLowerInvariant());

    // Check within same parent (or root level)
    query = query.Where(c =>
      parentId != null
        ? c.ParentCollectionId == parentId
        : c.ParentCollectionId == null);

    // Exclude specific collection (for updates)
    if (excludeId != null)
    {
      query = query.Where(c => c.Id != excludeId);
    }

    return await query.AnyAsync(cancellationToken);
  }

  /// <summary>
  /// Extracts ancestor IDs from hierarchy path
  /// Example: "/a/b/c/" -> [a, b] (excludes the collection itself)
  /// </summary>
  private static List<Guid> ExtractAncestorIds(string hierarchyPath)
  {
    var parts = hierarchyPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Exclude the last part (the collection itself)
    if (parts.Length <= 1)
    {
      return new List<Guid>();
    }

    var ancestorIds = new List<Guid>();
    for (int i = 0; i < parts.Length - 1; i++)
    {
      if (Guid.TryParse(parts[i], out var ancestorId))
      {
        ancestorIds.Add(ancestorId);
      }
    }

    return ancestorIds;
  }
}
