using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.ValueObjects;
using Org.BouncyCastle.Asn1.Icao;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for CollectionItem entity
/// </summary>
public class CollectionItemConfiguration : IEntityTypeConfiguration<CollectionItem>
{
  public void Configure(EntityTypeBuilder<CollectionItem> builder)
  {
    builder.ToTable("CollectionItems", "dbo");

    // Primary key
    builder.HasKey(ci => ci.Id);

    builder.Property(ci => ci.Id)
      .HasConversion(
        id => id.Value,
        value => CollectionItemId.Create(value))
      .HasColumnName("CollectionItemId")
      .IsRequired();

    // Shadow property for foreign key to Collection
    builder.Property<CollectionId>("CollectionId")
      .HasConversion(
        id => id.Value,
        value => CollectionId.Create(value))
      .IsRequired();

    // Properties
    builder.Property(ci => ci.ItemType)
      .HasConversion<byte>()
      .IsRequired();

    builder.Property(ci => ci.ItemReferenceId)
      .IsRequired();

    builder.Property(ci => ci.Order)
      .HasColumnName("OrderIndex")
      .IsRequired();

    builder.Property(ci => ci.AddedBy)
      .IsRequired();

    builder.Property(ci => ci.AddedAt)
      .IsRequired();

    // Indexes
    builder.HasIndex("CollectionId", "Order")
      .HasDatabaseName("IX_CollectionItems_CollectionId");

    builder.HasIndex(ci => new { ci.ItemType, ci.ItemReferenceId })
      .HasDatabaseName("IX_CollectionItems_ItemReference");

    // Unique constraint: same item can't be in same collection twice
    builder.HasIndex("CollectionId", "ItemReferenceId")
      .IsUnique()
      .HasDatabaseName("UQ_CollectionItems_Item");
  }
}
