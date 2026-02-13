using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.AuditAggregate;

namespace Nexus.API.Infrastructure.Data.Config;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "audit");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd(); // BIGINT IDENTITY

        builder.Property(a => a.UserId)
            .IsRequired(false);

        builder.Property(a => a.UserEmail)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(a => a.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45) // IPv6 max length
            .IsRequired(false);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(a => a.NewValues)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(a => a.AdditionalData)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(a => new { a.UserId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_UserId_Timestamp")
            .IsDescending(false, true);

        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_Entity_Timestamp")
            .IsDescending(false, false, true);

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp")
            .IsDescending();

        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}

public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
{
    public void Configure(EntityTypeBuilder<SecurityLog> builder)
    {
        builder.ToTable("SecurityLogs", "audit");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd(); // BIGINT IDENTITY

        builder.Property(s => s.UserId)
            .IsRequired(false);

        builder.Property(s => s.EventType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Timestamp)
            .IsRequired();

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.Success)
            .IsRequired();

        builder.Property(s => s.FailureReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.AdditionalData)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => new { s.UserId, s.Timestamp })
            .HasDatabaseName("IX_SecurityLogs_UserId_Timestamp")
            .IsDescending(false, true);

        builder.HasIndex(s => new { s.EventType, s.Timestamp })
            .HasDatabaseName("IX_SecurityLogs_EventType_Timestamp")
            .IsDescending(false, true);

        builder.HasIndex(s => s.Timestamp)
            .HasDatabaseName("IX_SecurityLogs_Timestamp")
            .IsDescending();

        builder.HasIndex(s => s.Success)
            .HasDatabaseName("IX_SecurityLogs_Success")
            .HasFilter("[Success] = 0"); // Index only failures

        // Ignore domain events
        builder.Ignore(s => s.DomainEvents);
    }
}
