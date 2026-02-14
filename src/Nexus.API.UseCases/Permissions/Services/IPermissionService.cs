using Nexus.API.Core.Aggregates.ResourcePermissions;

namespace Nexus.API.UseCases.Permissions.Services;

/// <summary>
/// Application-level permission service.
/// Provides a convenient facade over the raw IPermissionRepository
/// for use by other feature handlers (Documents, Diagrams, Snippets).
///
/// Register as Scoped in DI.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Returns true when the user has at least the given permission level
    /// on the resource, OR when the user is the resource creator (implicit Owner).
    /// </summary>
    Task<bool> CanAccessAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel minimumLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when the user can view the resource (Viewer or above).
    /// </summary>
    Task<bool> CanViewAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => CanAccessAsync(resourceType, resourceId, userId, PermissionLevel.Viewer, cancellationToken);

    /// <summary>
    /// Returns true when the user can edit the resource (Editor or above).
    /// </summary>
    Task<bool> CanEditAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => CanAccessAsync(resourceType, resourceId, userId, PermissionLevel.Editor, cancellationToken);

    /// <summary>
    /// Returns true when the user can manage permissions (Admin or above).
    /// </summary>
    Task<bool> CanManagePermissionsAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => CanAccessAsync(resourceType, resourceId, userId, PermissionLevel.Admin, cancellationToken);
}
