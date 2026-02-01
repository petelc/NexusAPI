using FastEndpoints;
using FastEndpoints.Swagger;
using Nexus.API.Infrastructure;
using Nexus.API.Web.Configurations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddInfrastructureServices(builder.Configuration, Log.Logger);
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddleware();

// NOTE: Removed auto-migration and initialization
// For production: Use manual migrations via "dotnet ef database update"
// For development: Migrations run automatically in docker-compose setup
//
// If you need to run migrations on startup (NOT recommended for production):
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     await dbContext.Database.MigrateAsync();
// }

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
