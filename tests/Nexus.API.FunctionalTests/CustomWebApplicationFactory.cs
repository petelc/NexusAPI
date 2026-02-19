using Nexus.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Infrastructure.Identity;

namespace Nexus.API.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithPassword("Your_password123!")
    .Build();

  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();
  }

  public new async Task DisposeAsync()
  {
    await _dbContainer.DisposeAsync();
  }

  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Testing"); // will not send real emails
    var host = builder.Build();
    host.Start();

    var serviceProvider = host.Services;

    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      try
      {
        // AppDbContext has no EF migrations yet — use EnsureCreated to create
        // the schema directly from the current model.
        var appDb = scopedServices.GetRequiredService<AppDbContext>();
        appDb.Database.EnsureCreated();

        // IdentityDbContext has code-first migrations; apply them.
        var identityDb = scopedServices.GetRequiredService<IdentityDbContext>();
        identityDb.Database.Migrate();

        // Seed Identity roles and test user
        SeedTestDataAsync(scopedServices).GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test data. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureServices(services =>
        {
          // Remove the app's AppDbContext registration
          var appDbDescriptors = services.Where(
            d => d.ServiceType == typeof(AppDbContext) ||
                 d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

          foreach (var descriptor in appDbDescriptors)
          {
            services.Remove(descriptor);
          }

          // Remove the IdentityDbContext registration
          var identityDbDescriptors = services.Where(
            d => d.ServiceType == typeof(IdentityDbContext) ||
                 d.ServiceType == typeof(DbContextOptions<IdentityDbContext>))
                .ToList();

          foreach (var descriptor in identityDbDescriptors)
          {
            services.Remove(descriptor);
          }

          var connectionString = _dbContainer.GetConnectionString();

          // Add both DbContexts using the Testcontainers SQL Server instance
          services.AddDbContext<AppDbContext>((provider, options) =>
          {
            options.UseSqlServer(connectionString);
          });

          services.AddDbContext<IdentityDbContext>((provider, options) =>
          {
            options.UseSqlServer(connectionString);
          });
        });
  }

  private static async Task SeedTestDataAsync(IServiceProvider services)
  {
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed roles
    string[] roles = ["Viewer", "Editor", "Admin", "Guest"];
    foreach (var role in roles)
    {
      if (!await roleManager.RoleExistsAsync(role))
      {
        await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
      }
    }

    // Seed a test user
    var testUser = await userManager.FindByEmailAsync(TestConstants.TestUserEmail);
    if (testUser == null)
    {
      testUser = new ApplicationUser
      {
        Id = Guid.NewGuid(),
        Email = TestConstants.TestUserEmail,
        UserName = TestConstants.TestUserUsername,
        FirstName = TestConstants.TestUserFirstName,
        LastName = TestConstants.TestUserLastName,
        EmailConfirmed = true,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
      };

      var result = await userManager.CreateAsync(testUser, TestConstants.TestUserPassword);
      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(testUser, "Editor");
        await userManager.AddToRoleAsync(testUser, "Admin");
      }
    }
  }
}

/// <summary>
/// Constants shared across functional tests for the seeded test user.
/// </summary>
public static class TestConstants
{
  public const string TestUserEmail = "testuser@nexus.dev";
  public const string TestUserPassword = "TestPass123!";
  public const string TestUserUsername = "testuser";
  public const string TestUserFirstName = "Test";
  public const string TestUserLastName = "User";
}
