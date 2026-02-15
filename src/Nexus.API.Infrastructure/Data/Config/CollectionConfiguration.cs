using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Collection aggregate
/// </summary>
public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
  public void Configure(EntityTypeBuilder<Collection> builder)
  {
    builder.ToTable("Collections", "dbo");

    // Primary key
    builder.HasKey(c => c.Id);

    builder.Property(c => c.Id)
      .HasConversion(
        id => id.Value,
        value => CollectionId.Create(value))
      .HasColumnName("CollectionId")
      .IsRequired();

    // Properties
    builder.Property(c => c.Name)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(c => c.Description)
      .HasMaxLength(1000)
      .IsRequired(false);

    builder.Property(c => c.Icon)
      .HasMaxLength(50)
      .IsRequired(false);

    builder.Property(c => c.Color)
      .HasMaxLength(7)
      .IsRequired(false);

    builder.Property(c => c.OrderIndex)
      .IsRequired();

    builder.Property(c => c.IsDeleted)
      .IsRequired()
      .HasDefaultValue(false);

    builder.Property(c => c.DeletedAt)
      .IsRequired(false);

    builder.Property(c => c.CreatedBy)
      .IsRequired();

    builder.Property(c => c.CreatedAt)
      .IsRequired();

    builder.Property(c => c.UpdatedAt)
      .IsRequired();

    // WorkspaceId - Strong-typed ID
    builder.Property(c => c.WorkspaceId)
      .HasConversion(
        id => id.Value,
        value => WorkspaceId.Create(value))
      .HasColumnName("WorkspaceId")
      .IsRequired();

    // ParentCollectionId - Nullable Strong-typed ID
    builder.Property(c => c.ParentCollectionId)
      .HasConversion(
        id => id != null ? id.Value : (Guid?)null,
        value => value.HasValue ? CollectionId.Create(value.Value) : null)
      .HasColumnName("ParentCollectionId")
      .IsRequired(false);

    // HierarchyPath - Value Object stored as string + int
    builder.OwnsOne(c => c.HierarchyPath, hp =>
    {
      hp.Property(p => p.Value)
        .HasColumnName("HierarchyPath")
        .HasMaxLength(4000)
        .IsRequired();

      hp.Property(p => p.Level)
        .HasColumnName("HierarchyLevel")
        .IsRequired();

      hp.HasIndex(p => p.Value)
        .HasDatabaseName("IX_Collections_HierarchyPath");
    });

    // Self-referencing relationship (Parent-Child)
    builder.HasOne<Collection>()
      .WithMany()
      .HasForeignKey(c => c.ParentCollectionId)
      .OnDelete(DeleteBehavior.Restrict);

    // Items collection (one-to-many)
    builder.HasMany(c => c.Items)
      .WithOne()
      .HasForeignKey("CollectionId")
      .OnDelete(DeleteBehavior.Cascade);

    // Indexes
    builder.HasIndex(c => c.WorkspaceId)
      .HasDatabaseName("IX_Collections_WorkspaceId");

    builder.HasIndex(c => c.ParentCollectionId)
      .HasDatabaseName("IX_Collections_ParentCollectionId");

    builder.HasIndex(c => new { c.ParentCollectionId, c.OrderIndex })
      .HasDatabaseName("IX_Collections_OrderIndex");

    // Query filter for soft delete
    builder.HasQueryFilter(c => !c.IsDeleted);
  }
}
