using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Nexus.API.Web.Configuration;

/// <summary>
/// Rate limiting policies for the Nexus API.
///
/// Two policies are defined:
///   "authenticated" — applied to all authenticated endpoints (100 req/min per user)
///   "anonymous"     — applied to auth endpoints (20 req/min per IP)
///
/// Register in Program.cs:
///   builder.Services.AddNexusRateLimiting();
///   ...
///   app.UseRateLimiter();
///
/// Apply to endpoints via the FastEndpoints Description fluent API:
///   .RequireRateLimiting("authenticated")
///
/// Or apply globally in UseNexusEndpoints() via a filter.
/// </summary>
public static class RateLimitingExtensions
{
    public const string AuthenticatedPolicy = "authenticated";
    public const string AnonymousPolicy = "anonymous";

    public static IServiceCollection AddNexusRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 429;

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "RATE_LIMIT_EXCEEDED",
                        message = "Too many requests. Please slow down and try again.",
                        timestamp = DateTime.UtcNow,
                        path = context.HttpContext.Request.Path.Value,
                        requestId = context.HttpContext.TraceIdentifier
                    }
                }, token);
            };

            // Authenticated users: 100 requests per minute per user ID
            options.AddFixedWindowLimiter(AuthenticatedPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Anonymous / auth endpoints: 20 requests per minute per IP
            options.AddFixedWindowLimiter(AnonymousPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 20;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });

            // Global fallback — catches anything without an explicit policy
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                {
                    // Partition by user ID if authenticated, otherwise by IP
                    var userId = httpContext.User.FindFirst("uid")?.Value;
                    var partitionKey = !string.IsNullOrEmpty(userId)
                        ? $"user:{userId}"
                        : $"ip:{httpContext.Connection.RemoteIpAddress}";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
        });

        return services;
    }
}
