using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Collaboration;
using Nexus.API.Infrastructure.Data;
using Nexus.API.Infrastructure.Data.Repositories;
using Nexus.API.Infrastructure.Repositories;
using Nexus.API.Infrastructure.Services;
using Traxs.SharedKernel;
using Elastic.Clients.Elasticsearch;

namespace Nexus.API.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        // Database Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("DetailedErrors"))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        var elasticsearchUrl = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        var elasticSettings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DefaultIndex("nexus-content");

        // Repository Registration
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Nexus.API.Infrastructure.Data.RepositoryBase<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpContextAccessor(); // Required for CurrentUserService

        // Add Tag repository
        services.AddScoped<ITagRepository, TagRepository>();
        // Register Diagram Repository
        services.AddScoped<IDiagramRepository, DiagramRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();

        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();

        services.AddScoped<ICollaborationRepository, CollaborationRepository>();

        // ===== Phase 3A: Real-Time Collaboration Services =====

        // Connection Manager (Singleton for in-memory tracking)
        // For production scale-out, use Redis-based implementation
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddSingleton(new ElasticsearchClient(elasticSettings));

        services.AddSingleton<ISearchService, ElasticsearchService>();

        services.AddScoped<IAuditService, AuditService>();

        // ======================================================

        // External Services (optional - only register if configured)
        AddExternalServices(services, configuration, logger);

        logger.LogInformation("Infrastructure services registered successfully");

        return services;
    }

    private static void AddExternalServices(
        IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        // Redis Cache (optional)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
                    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection));
                services.AddSingleton<ICacheService, Services.RedisCacheService>();
                logger.LogInformation("Redis cache service registered");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to register Redis cache service, continuing without cache");
            }
        }
        else
        {
            logger.LogInformation("Redis not configured, skipping cache service registration");
        }

        // Azure Blob Storage (optional)
        var blobConnectionString = configuration.GetConnectionString("AzureStorage");
        if (!string.IsNullOrEmpty(blobConnectionString))
        {
            try
            {
                services.AddSingleton(sp =>
                    new Azure.Storage.Blobs.BlobServiceClient(blobConnectionString));
                services.AddScoped<IStorageService, Services.BlobStorageService>();
                logger.LogInformation("Azure Blob Storage service registered");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to register Blob Storage service, continuing without file storage");
            }
        }
        else
        {
            logger.LogInformation("Azure Storage not configured, skipping blob storage service registration");
        }

        // Elasticsearch (optional)
        var elasticUri = configuration["Elasticsearch:Uri"];
        if (!string.IsNullOrEmpty(elasticUri))
        {
            try
            {
                services.AddSingleton<ISearchService, Services.ElasticsearchService>();
                logger.LogInformation("Elasticsearch service registered");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to register Elasticsearch service, continuing without search");
            }
        }
        else
        {
            logger.LogInformation("Elasticsearch not configured, skipping search service registration");
        }

        // Email Service (always registered, uses SMTP configuration)
        try
        {
            services.AddScoped<IEmailService, Services.EmailService>();
            logger.LogInformation("Email service registered");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to register Email service");
        }
    }

    /// <summary>
    /// Apply database migrations automatically on startup
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    /// <summary>
    /// Initialize external services (Elasticsearch indexes, etc.)
    /// Call this after ApplyMigrationsAsync in Program.cs
    /// </summary>
    public static async Task InitializeExternalServicesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Initialize Elasticsearch indexes if configured
            var searchService = scope.ServiceProvider.GetService<ISearchService>();
            if (searchService != null)
            {
                logger.LogInformation("Elasticsearch service is available and ready");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing external services");
            // Don't throw - allow application to start even if external services fail
        }
    }
}
