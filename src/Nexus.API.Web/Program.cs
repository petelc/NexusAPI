using FastEndpoints;
using FastEndpoints.Swagger;
using Nexus.API.Infrastructure;
using Nexus.API.Web.Configurations;
using Nexus.API.UseCases;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;

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

// Add services
builder.Services.AddInfrastructureServices(builder.Configuration, logger);
builder.Services.AddUseCasesServices();  // Add this line

// Add CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("NexusCorsPolicy", policy =>
  {
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
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
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
  };
});

builder.Services.AddAuthorization();

// Add Health Checks
builder.Services.AddHealthChecks();

// FastEndpoints and Swagger
builder.Services.AddFastEndpoints();
// .SwaggerDocument(o =>
//     {
//       o.ShortSchemaNames = true;

//       o.DocumentSettings = settings =>
//       {
//         settings.Title = "NEXUS API";
//         settings.Version = "v1";
//         settings.Description = "A production-ready  SaaS platform with authentication";

//         settings.PostProcess = document =>
//           {
//             document.Servers.Clear();

//             // document.Servers.Add(new NSwag.OpenApiServer
//             // {
//             //   Url = "https://localhost:5000",
//             //   Description = "Acme Tenant"
//             // });

//             document.Servers.Add(new NSwag.OpenApiServer
//             {
//               Url = "https://localhost:57679",
//               Description = "Base URL"
//             });
//           };
//       };
//     });
builder.Services.SwaggerDocument(o =>
{
  o.DocumentSettings = s =>
  {
    s.Title = "Nexus API";
    s.Version = "v1";
    s.Description = "A production-ready  SaaS platform with authentication";


    s.PostProcess = document =>
      {
        document.Servers.Clear();

        // document.Servers.Add(new NSwag.OpenApiServer
        // {
        //   Url = "https://localhost:5000",
        //   Description = "Acme Tenant"
        // });

        document.Servers.Add(new NSwag.OpenApiServer
        {
          Url = "https://localhost:57679",
          Description = "Base URL"
        });
      };
  };
});

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddleware();

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