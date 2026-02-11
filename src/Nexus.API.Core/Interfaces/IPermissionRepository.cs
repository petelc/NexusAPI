using Nexus.API.Core.Aggregates.ResourcePermissions;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for ResourcePermission entities.
/// </summary>
public interface IPermissionRepository //: IRepository<ResourcePermission>
{
    /// <summary>
    /// Returns all permissions for a specific resource.
    /// </summary>
    Task<IReadOnlyList<ResourcePermission>> GetByResourceAsync(
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the permission a specific user has on a specific resource,
    /// or null if no grant exists.
    /// </summary>
    Task<ResourcePermission?> GetByResourceAndUserAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all permissions granted to a user across all resources.
    /// </summary>
    Task<IReadOnlyList<ResourcePermission>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a permission by its ID.
    /// </summary>
    Task<ResourcePermission?> GetByIdAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new permission grant.
    /// </summary>
    Task<ResourcePermission> AddAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission grant.
    /// </summary>
    Task UpdateAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default);

    /// <summary> 
    /// Deletes a permission grant.
    /// </summary>
    Task DeleteAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user holds at least the given level on a resource.
    /// Considers resource ownership (CreatedBy) as implicitly Owner level.
    /// </summary>
    Task<bool> HasPermissionAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel minimumLevel,
        CancellationToken cancellationToken = default);
}
