using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Entities;
using Nexus.API.Infrastructure.Data.Config;
using Traxs.SharedKernel;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Aggregates.AuditAggregate;
using Nexus.API.Core.Interfaces;



namespace Nexus.API.Infrastructure.Data;

/// <summary>
/// Main application database context
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
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
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<CollaborationSession> CollaborationSessions => Set<CollaborationSession>();
    public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<SessionChange> SessionChanges => Set<SessionChange>();

    public DbSet<ResourcePermission> ResourcePermissions => Set<ResourcePermission>();

    // Add RefreshToken DbSet
    //public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SecurityLog> SecurityLogs => Set<SecurityLog>();

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
        modelBuilder.ApplyConfiguration(new WorkspaceConfiguration());

        // Add RefreshToken configuration
        //modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.ApplyConfiguration(new CollaborationSessionConfiguration());
        modelBuilder.ApplyConfiguration(new SessionParticipantConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new SessionChangeConfiguration());

        modelBuilder.ApplyConfiguration(new ResourcePermissionConfiguration());

        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityLogConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Populate AddedBy on new DocumentTags join entries
        if (_currentUserService?.UserId is not null)
        {
            var userId = _currentUserService.UserId.Value.Value;
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Metadata.GetTableName() == "DocumentTags"))
            {
                entry.Property("AddedBy").CurrentValue = userId;
            }
        }

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
