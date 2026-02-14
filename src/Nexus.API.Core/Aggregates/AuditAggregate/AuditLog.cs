namespace Nexus.API.Core.Aggregates.AuditAggregate;

/// <summary>
/// Represents an audit log entry for tracking user actions on resources.
/// Captures Create, Update, Delete, View, Share, and other operations.
///
/// Table: audit.AuditLogs
/// </summary>
public class AuditLog : EntityBase<long>
{
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string EntityType { get; private set; } = string.Empty; // "Document", "Diagram", "Snippet", etc.
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty; // "Create", "Update", "Delete", "View", "Share", etc.
    public DateTime Timestamp { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? OldValues { get; private set; } // JSON
    public string? NewValues { get; private set; } // JSON
    public string? AdditionalData { get; private set; } // JSON

    // EF Core constructor
    private AuditLog() { }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public static AuditLog Create(
        Guid? userId,
        string? userEmail,
        string entityType,
        Guid entityId,
        string action,
        string? ipAddress = null,
        string? userAgent = null,
        string? oldValues = null,
        string? newValues = null,
        string? additionalData = null)
    {
        return new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OldValues = oldValues,
            NewValues = newValues,
            AdditionalData = additionalData
        };
    }
}
