using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Infrastructure.Middleware;

/// <summary>
/// Middleware that automatically logs API requests to audit.AuditLogs.
/// 
/// Captures:
/// - User ID and email from JWT claims
/// - HTTP method and path
/// - IP address and User-Agent
/// - Request/response timing
///
/// Only logs for authenticated requests with specific HTTP methods.
///
/// Register in Program.cs AFTER UseAuthentication():
///   app.UseMiddleware<AuditMiddleware>();
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AuditableMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH", "DELETE"
    };

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Only audit state-changing operations
        if (!AuditableMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Only audit authenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Extract user information from JWT claims
        var userId = context.User.FindFirstValue("uid");
        var userEmail = context.User.FindFirstValue("email");
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        // Extract entity info from path
        var (entityType, entityId) = ParseEntityFromPath(context.Request.Path);

        // Determine action from HTTP method
        var action = context.Request.Method switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Unknown"
        };

        // Execute the request
        await _next(context);

        // Log after successful completion (only log 2xx responses)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            // Only log if we successfully parsed entity info
            if (!string.IsNullOrEmpty(entityType) && entityId != Guid.Empty)
            {
                var additionalData = JsonSerializer.Serialize(new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    StatusCode = context.Response.StatusCode
                });

                await auditService.LogAuditAsync(
                    userId != null ? Guid.Parse(userId) : null,
                    userEmail,
                    entityType,
                    entityId,
                    action,
                    oldValues: null, // Would require reading request body
                    newValues: null, // Would require reading response body
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    additionalData: additionalData
                );
            }
        }
    }

    /// <summary>
    /// Attempts to parse entity type and ID from the request path.
    /// Examples:
    ///   /api/v1/documents/{id} → ("Document", {id})
    ///   /api/v1/diagrams/{id}/elements/{elementId} → ("Diagram", {id})
    /// </summary>
    private static (string EntityType, Guid EntityId) ParseEntityFromPath(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // Look for pattern: /api/v1/{entityType}/{guid}
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i].Equals("v1", StringComparison.OrdinalIgnoreCase) && i + 2 < segments.Length)
            {
                var entityType = segments[i + 1]; // e.g., "documents"
                var idSegment = segments[i + 2];

                if (Guid.TryParse(idSegment, out var entityId))
                {
                    // Normalize to singular form
                    var normalized = entityType.TrimEnd('s');
                    return (CapitalizeFirst(normalized), entityId);
                }
            }
        }

        return (string.Empty, Guid.Empty);
    }

    private static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
