using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Layer entity
/// </summary>
public class LayerConfiguration : IEntityTypeConfiguration<Layer>
{
  public void Configure(EntityTypeBuilder<Layer> builder)
  {
    builder.ToTable("DiagramLayers", "dbo");

    // Primary Key
    builder.HasKey(l => l.Id);

    builder.Property(l => l.Id)
      .HasConversion(
        id => id.Value,
        value => LayerId.Create(value))
      .HasColumnName("LayerId")
      .IsRequired();

    // Foreign Key to Diagram (shadow property)
    builder.Property<DiagramId>("DiagramId")
      .HasConversion(
        id => id.Value,
        value => DiagramId.Create(value))
      .IsRequired();

    // Name
    builder.Property(l => l.Name)
      .HasMaxLength(100)
      .IsRequired();

    // Order
    builder.Property(l => l.Order)
      .IsRequired();

    // IsVisible
    builder.Property(l => l.IsVisible)
      .IsRequired()
      .HasDefaultValue(true);

    // IsLocked
    builder.Property(l => l.IsLocked)
      .IsRequired()
      .HasDefaultValue(false);

    // Indexes
    builder.HasIndex("DiagramId", "Order")
      .HasDatabaseName("IX_DiagramLayers_DiagramId_Order")
      .IsUnique();

    builder.HasIndex("DiagramId")
      .HasDatabaseName("IX_DiagramLayers_DiagramId");

    // Constraints
    builder.ToTable(t =>
    {
      t.HasCheckConstraint("CK_DiagramLayers_Order",
        "[Order] >= 0");
    });

    // Ignore domain events
    builder.Ignore(l => l.DomainEvents);
  }
}
