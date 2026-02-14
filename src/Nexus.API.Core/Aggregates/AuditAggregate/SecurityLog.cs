namespace Nexus.API.Core.Aggregates.AuditAggregate;

/// <summary>
/// Represents a security event log entry for authentication and authorization events.
/// Tracks logins, logouts, failed attempts, password changes, etc.
///
/// Table: audit.SecurityLogs
/// </summary>
public class SecurityLog : EntityBase<long>
{
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty; // "Login", "Logout", "FailedLogin", "PasswordChange", etc.
    public DateTime Timestamp { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }
    public string? AdditionalData { get; private set; } // JSON

    // EF Core constructor
    private SecurityLog() { }

    /// <summary>
    /// Creates a new security log entry.
    /// </summary>
    public static SecurityLog Create(
        Guid? userId,
        string eventType,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        string? additionalData = null)
    {
        return new SecurityLog
        {
            UserId = userId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason,
            AdditionalData = additionalData
        };
    }
}
