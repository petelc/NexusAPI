using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexus.API.Infrastructure.Data;

/// <summary>
/// Design-time factory for IdentityDbContext to support EF Core migrations
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=NexusIdentity;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly("Nexus.API.Infrastructure"));

        return new IdentityDbContext(optionsBuilder.Options);
    }
}