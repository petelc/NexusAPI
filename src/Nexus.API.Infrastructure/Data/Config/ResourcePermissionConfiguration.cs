using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.ResourcePermissions;

namespace Nexus.API.Infrastructure.Data.Config;

public class ResourcePermissionConfiguration : IEntityTypeConfiguration<ResourcePermission>
{
    public void Configure(EntityTypeBuilder<ResourcePermission> builder)
    {
        builder.ToTable("ResourcePermissions", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(p => p.ResourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.ResourceId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Level)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.GrantedBy)
            .IsRequired();

        builder.Property(p => p.GrantedAt)
            .IsRequired();

        builder.Property(p => p.ExpiresAt)
            .IsRequired(false);

        // Ignore computed properties — they are derived from persisted values
        builder.Ignore(p => p.IsOwner);
        builder.Ignore(p => p.CanEdit);
        builder.Ignore(p => p.CanComment);
        builder.Ignore(p => p.CanView);
        builder.Ignore(p => p.CanManagePermissions);
        builder.Ignore(p => p.IsExpired);
        builder.Ignore(p => p.IsValid);

        // A user may only have one active permission per resource
        builder.HasIndex(p => new { p.ResourceType, p.ResourceId, p.UserId })
            .IsUnique()
            .HasDatabaseName("UQ_ResourcePermissions_ResourceUser");

        // Fast lookups by resource
        builder.HasIndex(p => new { p.ResourceType, p.ResourceId })
            .HasDatabaseName("IX_ResourcePermissions_Resource");

        // Fast lookups by user
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_ResourcePermissions_UserId");
    }
}
