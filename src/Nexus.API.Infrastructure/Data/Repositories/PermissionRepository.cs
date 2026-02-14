using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<ResourcePermission>> GetByResourceAsync(
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .Where(p => p.ResourceType == resourceType && p.ResourceId == resourceId)
            .OrderBy(p => p.Level)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResourcePermission?> GetByResourceAndUserAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .FirstOrDefaultAsync(
                p => p.ResourceType == resourceType
                     && p.ResourceId == resourceId
                     && p.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ResourcePermission>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.ResourceType)
            .ThenBy(p => p.ResourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResourcePermission?> GetByIdAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourcePermissions
            .FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken);
    }

    public async Task<ResourcePermission> AddAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default)
    {
        _context.ResourcePermissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);
        return permission;
    }

    public async Task UpdateAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default)
    {
        _context.ResourcePermissions.Update(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        ResourcePermission permission,
        CancellationToken cancellationToken = default)
    {
        _context.ResourcePermissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel minimumLevel,
        CancellationToken cancellationToken = default)
    {
        // Check for an explicit permission grant at or above the minimum level
        var hasGrant = await _context.ResourcePermissions
            .AnyAsync(
                p => p.ResourceType == resourceType
                     && p.ResourceId == resourceId
                     && p.UserId == userId
                     && p.Level >= minimumLevel
                     && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow),
                cancellationToken);

        return hasGrant;
    }


}
