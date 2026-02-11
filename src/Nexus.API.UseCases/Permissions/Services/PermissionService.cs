using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Permissions.Services;

/// <summary>
/// Concrete implementation of IPermissionService.
/// Checks the ResourcePermissions table plus treats the resource creator
/// as an implicit Owner (so the creator never needs an explicit row).
///
/// This service does NOT know how to look up the creator of a resource —
/// callers pass in the creatorUserId when they have it, or use the
/// overloads that accept an IResourceOwnerResolver for automatic look-up.
///
/// Register as Scoped in DI:
///   services.AddScoped<IPermissionService, PermissionService>();
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissions;

    public PermissionService(IPermissionRepository permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }

    /// <inheritdoc />
    public async Task<bool> CanAccessAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel minimumLevel,
        CancellationToken cancellationToken = default)
    {
        // 1. Check an explicit grant row first (fast path)
        var grant = await _permissions.GetByResourceAndUserAsync(
            resourceType, resourceId, userId, cancellationToken);

        if (grant is not null && grant.IsValid && grant.Level >= minimumLevel)
            return true;

        // 2. No valid explicit grant — caller must rely on implicit-owner logic.
        //    The repository HasPermissionAsync is the authoritative check once
        //    a grant row exists. For implicit Owner (creator) the caller should
        //    short-circuit before invoking this service, or pass the creatorId
        //    variant below.
        return false;
    }

    /// <summary>
    /// Overload that also grants full access when <paramref name="userId"/>
    /// equals <paramref name="resourceCreatorId"/> (implicit Owner).
    /// Use this variant in handlers that already have the resource loaded.
    /// </summary>
    public Task<bool> CanAccessAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel minimumLevel,
        Guid resourceCreatorId,
        CancellationToken cancellationToken = default)
    {
        // Implicit owner — always has full access
        if (userId == resourceCreatorId)
            return Task.FromResult(true);

        return CanAccessAsync(resourceType, resourceId, userId, minimumLevel, cancellationToken);
    }
}
