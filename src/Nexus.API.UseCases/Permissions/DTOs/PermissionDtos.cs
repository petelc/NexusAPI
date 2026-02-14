using Nexus.API.Core.Aggregates.ResourcePermissions;

namespace Nexus.API.UseCases.Permissions.DTOs;

// ── Request DTOs ──────────────────────────────────────────────────────────────

/// <summary>
/// POST /api/v1/permissions — grant a permission
/// </summary>
public record GrantPermissionRequest(
    string ResourceType,
    Guid ResourceId,
    Guid UserId,
    string PermissionLevel,
    DateTime? ExpiresAt);

/// <summary>
/// PUT /api/v1/permissions/{id} — update permission level or expiry
/// </summary>
public record UpdatePermissionRequest(
    string PermissionLevel,
    DateTime? ExpiresAt);

// ── Response DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Represents a single permission grant in API responses.
/// </summary>
public record PermissionDto(
    Guid PermissionId,
    string ResourceType,
    Guid ResourceId,
    Guid UserId,
    string Level,
    bool IsOwner,
    bool CanEdit,
    bool CanComment,
    bool CanView,
    bool CanManagePermissions,
    Guid GrantedBy,
    DateTime GrantedAt,
    DateTime? ExpiresAt,
    bool IsExpired);

// ── Extension mapping ─────────────────────────────────────────────────────────

public static class PermissionMappingExtensions
{
    public static PermissionDto ToDto(this ResourcePermission p)
        => new(
            PermissionId: p.Id,
            ResourceType: p.ResourceType.ToString(),
            ResourceId: p.ResourceId,
            UserId: p.UserId,
            Level: p.Level.ToString(),
            IsOwner: p.IsOwner,
            CanEdit: p.CanEdit,
            CanComment: p.CanComment,
            CanView: p.CanView,
            CanManagePermissions: p.CanManagePermissions,
            GrantedBy: p.GrantedBy,
            GrantedAt: p.GrantedAt,
            ExpiresAt: p.ExpiresAt,
            IsExpired: p.IsExpired);
}
