using Microsoft.AspNetCore.Builder;
using Serilog;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace Nexus.API.Web.Configurations;

/// <summary>
/// Middleware configuration and pipeline setup
/// </summary>
public static class MiddlewareConfig
{
  public static WebApplication ConfigureMiddleware(this WebApplication app)
  {
    // Development-specific middleware
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      // Production error handling
      app.UseExceptionHandler("/error");
      app.UseHsts();
    }

    // Security headers
    app.Use(async (context, next) =>
    {
      context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
      context.Response.Headers.Append("X-Frame-Options", "DENY");
      context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
      context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
      await next();
    });

    // Request logging
    app.UseSerilogRequestLogging(options =>
    {
      options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
      options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
      {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
          diagnosticContext.Set("UserAgent", userAgent);
        }
      };
    });

    // HTTPS redirection
    app.UseHttpsRedirection();

    // CORS
    app.UseCors("NexusCorsPolicy");

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks
    app.MapHealthChecks("/health");

    // Swagger (FastEndpoints version) - available in all environments
    if (app.Environment.IsDevelopment())
    {
      app.UseSwaggerGen();
    }

    Log.Information("Middleware pipeline configured successfully");

    return app;
  }
}