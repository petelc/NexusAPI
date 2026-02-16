using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for SessionParticipant
/// </summary>
public class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
{
    public void Configure(EntityTypeBuilder<SessionParticipant> builder)
    {
        // Table mapping
        builder.ToTable("SessionParticipants", "collaboration");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ParticipantId.Create(value))
            .HasColumnName("Id")
            .ValueGeneratedNever(); // Generated in domain

        // Properties
        builder.Property(e => e.SessionId)
            .HasConversion(
                id => id.Value,
                value => SessionId.Create(value))
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.JoinedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.LeftAt)
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.LastActivityAt)
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.CursorPosition);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        // Relationships
        builder.HasOne(e => e.Session)
            .WithMany(s => s.Participants)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.SessionId)
            .HasDatabaseName("IX_SessionParticipants_SessionId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_SessionParticipants_UserId");

        builder.HasIndex(e => new { e.SessionId, e.UserId })
            .HasFilter("[LeftAt] IS NULL")
            .HasDatabaseName("IX_SessionParticipants_Active");
    }
}
