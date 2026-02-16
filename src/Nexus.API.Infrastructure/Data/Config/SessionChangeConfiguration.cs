using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for SessionChange
/// </summary>
public class SessionChangeConfiguration : IEntityTypeConfiguration<SessionChange>
{
    public void Configure(EntityTypeBuilder<SessionChange> builder)
    {
        // Table mapping
        builder.ToTable("SessionChanges", "collaboration");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ChangeId.Create(value))
            .HasColumnName("Id")
            .ValueGeneratedNever(); // Generated in domain

        // Properties
        builder.Property(e => e.SessionId)
            .HasConversion(
                id => id.Value,
                value => SessionId.Create(value))
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasConversion(
                id => id.Value,
                value => ParticipantId.Create(value))
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.ChangeType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Position)
            .IsRequired();

        builder.Property(e => e.Data)
            .HasMaxLength(4000); // NVARCHAR(MAX) in SQL

        builder.Property(e => e.ChangeHash)
            .HasMaxLength(32); // VARBINARY(32)

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        // Relationships
        builder.HasOne(e => e.Session)
            .WithMany(s => s.Changes)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.SessionId, e.Timestamp })
            .HasDatabaseName("IX_SessionChanges_SessionId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_SessionChanges_UserId");
    }
}
