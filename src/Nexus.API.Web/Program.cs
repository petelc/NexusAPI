using Nexus.API.Infrastructure;
using Nexus.API.UseCases;
using Nexus.API.Web.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Nexus.API.Infrastructure.Data;
using Nexus.API.Infrastructure.Identity;
using Ardalis.Specification;
using Nexus.API.Infrastructure.Services;
using Traxs.SharedKernel;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data.Repositories;
using Nexus.API.Web.Extensions;
using Nexus.API.Web.Hubs;
using Nexus.API.Web.Configuration;
using Nexus.API.Web.Middleware;
using Nexus.API.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

builder.Host.UseSerilog();

// Create Microsoft.Extensions.Logging.ILogger from the logger factory
using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSerilog());
var logger = loggerFactory.CreateLogger<Program>();

// Add Infrastructure services (includes DbContext, Repositories, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration, logger);

// Add UseCases services (includes MediatR and AutoMapper)
builder.Services.AddUseCasesServices();

// Identity DbContext (authentication)
builder.Services.AddDbContext<IdentityDbContext>(options =>
  options.UseSqlServer(
    builder.Configuration.GetConnectionString("IdentityConnection") ?? builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("Nexus.API.Infrastructure")));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
  // Password settings
  options.Password.RequireDigit = true;
  options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;
  options.Password.RequireNonAlphanumeric = true;
  options.Password.RequiredLength = 8;

  // Lockout settings
  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
  options.Lockout.MaxFailedAccessAttempts = 5;
  options.Lockout.AllowedForNewUsers = true;

  // User settings
  options.User.RequireUniqueEmail = true;

  // Email confirmation (disabled for now)
  options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// Add CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("NexusCorsPolicy", policy =>
  {
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:3000" })
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
  });
});

// Add Authentication & Authorization with JWT
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.MapInboundClaims = false;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
    RoleClaimType = "role",
    NameClaimType = "name"
  };
});

builder.Services.AddAuthorization();

// Register Application Services
// Repository pattern - using Traxs.SharedKernel
builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepositoryBase<>));
builder.Services.AddScoped(typeof(IReadRepositoryBase<>), typeof(EfRepositoryBase<>));

// Domain Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

// Add RefreshToken repository
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add RefreshToken repository
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Add Code Snippet repository
builder.Services.AddScoped<ICodeSnippetRepository, CodeSnippetRepository>();



builder.Services.AddScoped<ITeamRepository, TeamRepository>();

// Add UrlEncoder for 2FA QR code generation
builder.Services.AddSingleton<System.Text.Encodings.Web.UrlEncoder>(
  System.Text.Encodings.Web.UrlEncoder.Default);

// Add Health Checks
builder.Services.AddHealthChecks();

// Configure HttpContext JSON options so ReadFromJsonAsync (used by EndpointWithoutRequest
// endpoints) deserialises camelCase JSON from the frontend into PascalCase C# properties.
// PostConfigure runs after all other Configure() calls (including FastEndpoints) so our
// setting cannot be overridden.
builder.Services.PostConfigure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
  options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// FastEndpoints and Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
  o.MaxEndpointVersion = 1;
  o.AutoTagPathSegmentIndex = 0;
  o.DocumentSettings = s =>
  {
    s.Title = "Nexus API";
    s.Version = "v1";
    s.Description = "A production-ready SaaS platform with authentication";
  };
});

// Configure SignalR for real-time collaboration
builder.Services.AddSignalR(options =>
{
  options.EnableDetailedErrors = builder.Environment.IsDevelopment();
  options.MaximumReceiveMessageSize = 102_400; // 100 KB
  options.KeepAliveInterval = TimeSpan.FromSeconds(15);
  options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddCollaborationServices();

// Rate limiting (Phase A)
builder.Services.AddNexusRateLimiting();

var app = builder.Build();

// Global exception handler — must be FIRST in the pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure middleware pipeline
app.ConfigureMiddleware();

// Rate limiter — after exception handler, before auth
app.UseRateLimiter();

app.UseFastEndpoints(config =>
{
  config.Endpoints.RoutePrefix = "api/v1";  // ✅ Global prefix for ALL endpoints

  // Optional: Configure versioning
  config.Versioning.Prefix = "v";  // Results in /api/v1, /api/v2, etc.
  config.Versioning.PrependToRoute = false;  // Don't add version to individual routes
});

// Seed database (skip in Testing – the test factory handles migrations and seeding)
if (!app.Environment.IsEnvironment("Testing"))
{
  using (var scope = app.Services.CreateScope())
  {
    await SeedData.InitializeAsync(scope.ServiceProvider);
  }
}


app.UseMiddleware<AuditMiddleware>();


app.MapHub<CollaborationHub>("/hubs/collaboration");

try
{
  Log.Information("Starting Nexus API...");
  Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

  await app.RunAsync();

  Log.Information("Nexus API stopped gracefully");
  return 0;
}
catch (Exception ex)
{
  Log.Fatal(ex, "Nexus API terminated unexpectedly");
  return 1;
}
finally
{
  await Log.CloseAndFlushAsync();
}

public partial class Program { }