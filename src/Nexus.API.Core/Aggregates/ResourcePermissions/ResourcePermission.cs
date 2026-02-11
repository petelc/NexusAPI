using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.ResourcePermissions;

/// <summary>
/// Represents a permission grant for a specific resource (Document, Diagram, or CodeSnippet).
///
/// A single unified entity covers all resource types, avoiding three separate
/// DocumentPermission / DiagramPermission / CodeSnippetPermission tables
/// while keeping the domain model clean.
///
/// PermissionLevel hierarchy:
///   1 = Viewer   — can read
///   2 = Commenter— can read and comment
///   3 = Editor   — can read, comment, and edit
///   4 = Admin    — can read, comment, edit, and manage permissions
///   5 = Owner    — full control including deletion
/// </summary>
public class ResourcePermission : EntityBase<Guid>, IAggregateRoot
{
    // ── Identity ─────────────────────────────────────────────────────────────

    public ResourceType ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }

    // ── Who ──────────────────────────────────────────────────────────────────

    public Guid UserId { get; private set; }
    public PermissionLevel Level { get; private set; }

    // ── Audit ────────────────────────────────────────────────────────────────

    public Guid GrantedBy { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // ── Convenience helpers ───────────────────────────────────────────────────

    public bool IsOwner => Level == PermissionLevel.Owner;
    public bool CanEdit => Level >= PermissionLevel.Editor;
    public bool CanComment => Level >= PermissionLevel.Commenter;
    public bool CanView => Level >= PermissionLevel.Viewer;
    public bool CanManagePermissions => Level >= PermissionLevel.Admin;

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool IsValid => !IsExpired;

    // ── EF Core ───────────────────────────────────────────────────────────────

    private ResourcePermission() { }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static ResourcePermission Grant(
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        PermissionLevel level,
        Guid grantedBy,
        DateTime? expiresAt = null)
    {
        if (resourceId == Guid.Empty)
            throw new ArgumentException("Resource ID cannot be empty.", nameof(resourceId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (grantedBy == Guid.Empty)
            throw new ArgumentException("GrantedBy cannot be empty.", nameof(grantedBy));

        return new ResourcePermission
        {
            Id = Guid.NewGuid(),
            ResourceType = resourceType,
            ResourceId = resourceId,
            UserId = userId,
            Level = level,
            GrantedBy = grantedBy,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Elevates or reduces the permission level for this grant.
    /// </summary>
    public void ChangeLevel(PermissionLevel newLevel)
    {
        if (IsOwner)
            throw new InvalidOperationException(
                "Cannot change the permission level of an Owner grant. Transfer ownership instead.");

        Level = newLevel;
    }

    /// <summary>
    /// Sets or clears the expiry date.
    /// </summary>
    public void SetExpiry(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future.", nameof(expiresAt));

        ExpiresAt = expiresAt;
    }
}

/// <summary>
/// Identifies the type of resource a permission applies to.
/// Must stay in sync with the ResourceType used by CollaborationSession.
/// </summary>
public enum ResourceType
{
    Document = 0,
    Diagram = 1,
    CodeSnippet = 2
}

/// <summary>
/// Permission levels — numeric values determine hierarchy comparisons (>=).
/// </summary>
public enum PermissionLevel
{
    Viewer = 1,
    Commenter = 2,
    Editor = 3,
    Admin = 4,
    Owner = 5
}
