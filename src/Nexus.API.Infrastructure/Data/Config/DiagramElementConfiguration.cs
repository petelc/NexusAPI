using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for DiagramElement entity
/// </summary>
public class DiagramElementConfiguration : IEntityTypeConfiguration<DiagramElement>
{
  public void Configure(EntityTypeBuilder<DiagramElement> builder)
  {
    builder.ToTable("DiagramElements", "dbo");

    // Primary Key
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasConversion(
        id => id.Value,
        value => ElementId.Create(value))
      .HasColumnName("ElementId")
      .IsRequired();

    // Foreign Key to Diagram (shadow property)
    builder.Property<DiagramId>("DiagramId")
      .HasConversion(
        id => id.Value,
        value => DiagramId.Create(value))
      .IsRequired();

    // ShapeType (Enum)
    builder.Property(e => e.ShapeType)
      .HasConversion<int>()
      .IsRequired();

    // Position (Value Object)
    builder.OwnsOne(e => e.Position, position =>
    {
      position.Property(p => p.X)
        .HasColumnName("PositionX")
        .HasColumnType("decimal(10,2)")
        .IsRequired();

      position.Property(p => p.Y)
        .HasColumnName("PositionY")
        .HasColumnType("decimal(10,2)")
        .IsRequired();
    });

    // Size (Value Object)
    builder.OwnsOne(e => e.Size, size =>
    {
      size.Property(s => s.Width)
        .HasColumnName("Width")
        .HasColumnType("decimal(10,2)")
        .IsRequired();

      size.Property(s => s.Height)
        .HasColumnName("Height")
        .HasColumnType("decimal(10,2)")
        .IsRequired();
    });

    // Style (Value Object)
    builder.OwnsOne(e => e.Style, style =>
    {
      style.Property(s => s.FillColor)
        .HasColumnName("FillColor")
        .HasMaxLength(7)
        .IsRequired();

      style.Property(s => s.StrokeColor)
        .HasColumnName("StrokeColor")
        .HasMaxLength(7)
        .IsRequired();

      style.Property(s => s.StrokeWidth)
        .HasColumnName("StrokeWidth")
        .IsRequired();

      style.Property(s => s.FontSize)
        .HasColumnName("FontSize")
        .IsRequired();

      style.Property(s => s.FontFamily)
        .HasColumnName("FontFamily")
        .HasMaxLength(50)
        .IsRequired();

      style.Property(s => s.Opacity)
        .HasColumnName("Opacity")
        .HasColumnType("decimal(3,2)")
        .IsRequired();

      style.Property(s => s.Rotation)
        .HasColumnName("Rotation")
        .HasColumnType("decimal(5,2)")
        .IsRequired();
    });

    builder.Property(e => e.ZIndex)
      .IsRequired()
      .HasDefaultValue(0);

    // Text
    builder.Property(e => e.Text)
      .HasMaxLength(500)
      .IsRequired(false);

    // LayerId (nullable FK)
    builder.Property(e => e.LayerId)
      .HasConversion(
        id => id.HasValue ? id.Value.Value : (Guid?)null,
        value => value.HasValue ? LayerId.Create(value.Value) : null)
      .IsRequired(false);

    // IsLocked
    builder.Property(e => e.IsLocked)
      .IsRequired()
      .HasDefaultValue(false);

    // Custom Properties (JSON)
    builder.Property(e => e.CustomProperties)
      .HasColumnType("nvarchar(max)")
      .IsRequired(false);

    // Indexes
    builder.HasIndex("DiagramId")
      .HasDatabaseName("IX_DiagramElements_DiagramId");

    builder.HasIndex(e => e.LayerId)
      .HasDatabaseName("IX_DiagramElements_LayerId");

    builder.HasIndex("DiagramId", nameof(DiagramElement.ZIndex)) // Changed from "ZIndex" string
      .HasDatabaseName("IX_DiagramElements_DiagramId_ZIndex");



    // Constraints
    builder.ToTable(t =>
    {
      t.HasCheckConstraint("CK_DiagramElements_ShapeType",
        "[ShapeType] BETWEEN 0 AND 99");

      t.HasCheckConstraint("CK_DiagramElements_Size",
        "[Width] > 0 AND [Height] > 0");

      t.HasCheckConstraint("CK_DiagramElements_Rotation",
        "[Rotation] >= 0 AND [Rotation] < 360");

      t.HasCheckConstraint("CK_DiagramElements_Opacity",
        "[Opacity] >= 0 AND [Opacity] <= 1");
    });

    // Ignore domain events
    builder.Ignore(e => e.DomainEvents);
  }
}
