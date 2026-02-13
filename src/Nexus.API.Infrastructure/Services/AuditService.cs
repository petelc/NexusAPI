using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.AuditAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Implementation of IAuditService.
/// Lives in Infrastructure layer because it directly accesses AppDbContext.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task LogAuditAsync(
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
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.Create(
            userId,
            userEmail,
            entityType,
            entityId,
            action,
            ipAddress,
            userAgent,
            oldValues,
            newValues,
            additionalData);

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogSecurityAsync(
        Guid? userId,
        string eventType,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        var securityLog = SecurityLog.Create(
            userId,
            eventType,
            success,
            ipAddress,
            userAgent,
            failureReason,
            additionalData);

        _context.SecurityLogs.Add(securityLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                AuditLogId = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action,
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                AdditionalData = a.AdditionalData
            })
            .ToListAsync(cancellationToken);

        return logs;
    }

    public async Task<List<SecurityLogDto>> GetSecurityLogsAsync(
        Guid? userId = null,
        string? eventType = null,
        bool? success = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(s => s.EventType == eventType);

        if (success.HasValue)
            query = query.Where(s => s.Success == success.Value);

        if (startDate.HasValue)
            query = query.Where(s => s.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.Timestamp <= endDate.Value);

        var logs = await query
            .OrderByDescending(s => s.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SecurityLogDto
            {
                SecurityLogId = s.Id,
                UserId = s.UserId,
                EventType = s.EventType,
                Timestamp = s.Timestamp,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                Success = s.Success,
                FailureReason = s.FailureReason,
                AdditionalData = s.AdditionalData
            })
            .ToListAsync(cancellationToken);

        return logs;
    }
}
