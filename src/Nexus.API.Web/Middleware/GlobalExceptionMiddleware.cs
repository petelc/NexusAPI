using System.Net;
using System.Text.Json;

namespace Nexus.API.Web.Middleware;

/// <summary>
/// Global exception middleware that converts unhandled exceptions into the
/// spec-defined error envelope format:
///
/// {
///   "error": {
///     "code": "INTERNAL_ERROR",
///     "message": "...",
///     "details": [],
///     "timestamp": "...",
///     "path": "...",
///     "requestId": "..."
///   }
/// }
///
/// Register in Program.cs BEFORE app.UseAuthentication():
///   app.UseMiddleware&lt;GlobalExceptionMiddleware&gt;();
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
            return;

        var (statusCode, errorCode, message) = MapException(exception);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                code = errorCode,
                message = _env.IsDevelopment() ? exception.Message : message,
                details = _env.IsDevelopment()
                    ? new[] { new { field = "stackTrace", message = exception.StackTrace ?? string.Empty } }
                    : Array.Empty<object>(),
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value,
                requestId = context.TraceIdentifier
            }
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, _jsonOptions));
    }

    private static (HttpStatusCode status, string code, string message) MapException(Exception ex)
        => ex switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "AUTHORIZATION_FAILED",
                "You do not have permission to perform this action."),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "RESOURCE_NOT_FOUND",
                "The requested resource was not found."),

            ArgumentException or InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                ex.Message),

            OperationCanceledException => (
                HttpStatusCode.ServiceUnavailable,
                "SERVICE_UNAVAILABLE",
                "The request was cancelled."),

            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred.")
        };
}
