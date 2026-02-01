using Microsoft.AspNetCore.Builder;
using Serilog;

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
      
      // Swagger in development only
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexus API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Nexus API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
      });
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
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
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

    // FastEndpoints
    app.UseFastEndpoints(config =>
    {
      config.Endpoints.RoutePrefix = "api";
      config.Endpoints.ShortNames = true;
      config.Versioning.Prefix = "v";
      config.Versioning.DefaultVersion = 1;
    });

    // NOTE: Database seeding removed - use migrations and separate seeding strategy
    // For production, use: dotnet ef database update
    // For development seeding, create a separate admin endpoint or CLI tool

    Log.Information("Middleware pipeline configured successfully");

    return app;
  }
}
