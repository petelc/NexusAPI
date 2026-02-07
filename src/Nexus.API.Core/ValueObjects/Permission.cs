using Nexus.API.Core.Enums;

namespace Nexus.API.Core.ValueObjects;

// Permission Value Object
public class Permission : ValueObject
{
    public UserId UserId { get; private set; }
    public PermissionLevel Level { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public bool IsOwner => Level == PermissionLevel.Owner;
    public bool CanEdit => Level >= PermissionLevel.Editor;
    public bool CanView => Level >= PermissionLevel.Viewer;

    private Permission(UserId userId, PermissionLevel level, DateTime grantedAt, DateTime? expiresAt = null)
    {
        UserId = userId;
        Level = level;
        GrantedAt = grantedAt;
        ExpiresAt = expiresAt;
    }

    public static Permission Grant(UserId userId, PermissionLevel level)
    {
        return new Permission(userId, level, DateTime.UtcNow);
    }

    public bool IsValid()
    {
        return !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return Level;
    }
}