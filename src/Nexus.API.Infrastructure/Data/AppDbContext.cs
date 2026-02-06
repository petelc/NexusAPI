using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Entities;
using Nexus.API.Infrastructure.Data.Config;
using Traxs.SharedKernel;
using Nexus.API.Core.Aggregates.CollectionAggregate;


namespace Nexus.API.Infrastructure.Data;

/// <summary>
/// Main application database context
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Document Aggregate
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<User> Users => Set<User>();

    public DbSet<Diagram> Diagrams => Set<Diagram>();
    public DbSet<DiagramElement> DiagramElements => Set<DiagramElement>();
    public DbSet<DiagramConnection> DiagramConnections => Set<DiagramConnection>();
    public DbSet<Layer> DiagramLayers => Set<Layer>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();

    // Add RefreshToken DbSet
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.ApplyConfiguration(new DiagramConfiguration());
        modelBuilder.ApplyConfiguration(new DiagramElementConfiguration());
        modelBuilder.ApplyConfiguration(new DiagramConnectionConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionItemConfiguration());
        modelBuilder.ApplyConfiguration(new LayerConfiguration());



        // Add RefreshToken configuration
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = ChangeTracker.Entries<EntityBase<DocumentId>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            // TODO: Publish domain events via MediatR or messaging system
            await Task.CompletedTask;
        }
    }
}
