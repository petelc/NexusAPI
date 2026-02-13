namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Service for logging audit and security events.
/// Interface is in Core, implementation in Infrastructure.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event (document created, updated, deleted, etc.).
    /// </summary>
    Task LogAuditAsync(
        Guid? userId,
        string? userEmail,
        string entityType,
        Guid entityId,
        string action,
        string? ipAddress = null,
        string? userAgent = null,
        string? oldValues = null,
        string? newValues = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a security event (login, logout, failed login, etc.).
    /// </summary>
    Task LogSecurityAsync(
        Guid? userId,
        string eventType,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit logs for a specific entity.
    /// </summary>
    Task<List<AuditLogDto>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves security logs for a specific user.
    /// </summary>
    Task<List<SecurityLogDto>> GetSecurityLogsAsync(
        Guid? userId = null,
        string? eventType = null,
        bool? success = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for audit log entries.
/// </summary>
public class AuditLogDto
{
    public long AuditLogId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AdditionalData { get; set; }
}

/// <summary>
/// DTO for security log entries.
/// </summary>
public class SecurityLogDto
{
    public long SecurityLogId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? AdditionalData { get; set; }
}
