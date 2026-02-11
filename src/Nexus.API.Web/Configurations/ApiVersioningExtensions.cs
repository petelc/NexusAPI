namespace Nexus.API.Web.Configuration;

/// <summary>
/// Documents the /api/v1 routing convention used across all endpoints.
///
/// All FastEndpoints use the route prefix: /api/v1/{resource}
///
/// This file is informational — the actual prefix is already embedded
/// in each endpoint's Configure() route string:
///   Post("/api/v1/teams")
///   Get("/api/v1/documents/{id}")
///
/// Existing endpoints that still use /api/{resource} or /documents/{resource}
/// (without the /v1 segment) must be updated to include /api/v1/.
/// See MIGRATION_NOTES.md for the full list.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Route prefix applied to every versioned endpoint.
    /// </summary>
    public const string V1Prefix = "/api/v1";

    /// <summary>
    /// Registers FastEndpoints with the /api/v1 global route prefix
    /// so that any endpoint defining Get("documents/{id}") is automatically
    /// reachable at /api/v1/documents/{id}.
    ///
    /// Usage in Program.cs:
    ///   app.UseFastEndpoints(c =>
    ///   {
    ///       c.Endpoints.RoutePrefix = "api/v1";
    ///       c.Errors.UseProblemDetails();     // uses the standard error format
    ///   });
    ///
    /// NOTE: Endpoints in the Teams/Workspaces/Collaboration features already
    /// embed the full path (e.g. "/api/v1/teams"). To avoid doubling the prefix,
    /// remove the leading "/api/v1" from those routes after enabling this global prefix.
    /// The NEW endpoints in Phases B and C use short routes (e.g. "documents/{id}")
    /// so they work correctly with the global prefix.
    /// </summary>
    public static IApplicationBuilder UseNexusEndpoints(this WebApplication app)
    {
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api/v1";
            c.Endpoints.ShortNames = true;
            c.Versioning.Prefix = "v";
            c.Versioning.DefaultVersion = 1;

            c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
            {
                var errors = failures
                    .Select(f => new { field = f.PropertyName, message = f.ErrorMessage })
                    .ToList();

                return new
                {
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "One or more validation errors occurred.",
                        details = errors,
                        timestamp = DateTime.UtcNow,
                        path = ctx.Request.Path.Value,
                        requestId = ctx.TraceIdentifier
                    }
                };
            };
        });

        return app;
    }
}
