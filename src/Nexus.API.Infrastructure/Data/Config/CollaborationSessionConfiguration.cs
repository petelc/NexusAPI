using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for CollaborationSession
/// </summary>
public class CollaborationSessionConfiguration : IEntityTypeConfiguration<CollaborationSession>
{
    public void Configure(EntityTypeBuilder<CollaborationSession> builder)
    {
        // Table mapping
        builder.ToTable("Sessions", "collaboration");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => SessionId.Create(value))
            .ValueGeneratedNever(); // Generated in domain

        // Properties
        builder.Property(e => e.ResourceType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ResourceId)
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.EndedAt)
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        // Relationships
        builder.HasMany(e => e.Participants)
            .WithOne(p => p.Session)
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Changes)
            .WithOne(c => c.Session)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Session)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => new { e.ResourceType, e.ResourceId, e.IsActive })
            .HasDatabaseName("IX_Sessions_Resource");

        builder.HasIndex(e => e.StartedAt)
            .IsDescending()
            .HasDatabaseName("IX_Sessions_StartedAt");
    }
}
