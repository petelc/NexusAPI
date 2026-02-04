using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for DiagramConnection entity
/// </summary>
public class DiagramConnectionConfiguration : IEntityTypeConfiguration<DiagramConnection>
{
  public void Configure(EntityTypeBuilder<DiagramConnection> builder)
  {
    builder.ToTable("DiagramConnections", "dbo");

    // Primary Key
    builder.HasKey(c => c.Id);

    builder.Property(c => c.Id)
      .HasConversion(
        id => id.Value,
        value => ConnectionId.Create(value))
      .HasColumnName("ConnectionId")
      .IsRequired();

    // Foreign Key to Diagram (shadow property)
    builder.Property<DiagramId>("DiagramId")
      .HasConversion(
        id => id.Value,
        value => DiagramId.Create(value))
      .IsRequired();

    // Source Element ID
    builder.Property(c => c.SourceElementId)
      .HasConversion(
        id => id.Value,
        value => ElementId.Create(value))
      .IsRequired();

    // Target Element ID
    builder.Property(c => c.TargetElementId)
      .HasConversion(
        id => id.Value,
        value => ElementId.Create(value))
      .IsRequired();

    // ConnectionType (Enum)
    builder.Property(c => c.ConnectionType)
      .HasConversion<int>()
      .IsRequired();

    // Style (Value Object)
    builder.OwnsOne(c => c.Style, style =>
    {
      style.Property(s => s.StrokeColor)
        .HasColumnName("StrokeColor")
        .HasMaxLength(7)
        .IsRequired();

      style.Property(s => s.StrokeWidth)
        .HasColumnName("StrokeWidth")
        .IsRequired();

      style.Property(s => s.StrokeDashArray)
        .HasColumnName("StrokeDashArray")
        .HasMaxLength(50)
        .IsRequired(false);
    });

    // Label
    builder.Property(c => c.Label)
      .HasMaxLength(200)
      .IsRequired(false);

    // Control Points (JSON for curves)
    builder.Property(c => c.ControlPoints)
      .HasColumnType("nvarchar(max)")
      .IsRequired(false);

    // Indexes
    builder.HasIndex("DiagramId")
      .HasDatabaseName("IX_DiagramConnections_DiagramId");

    builder.HasIndex(c => c.SourceElementId)
      .HasDatabaseName("IX_DiagramConnections_SourceElement");

    builder.HasIndex(c => c.TargetElementId)
      .HasDatabaseName("IX_DiagramConnections_TargetElement");

    // Constraints
    builder.ToTable(t =>
    {
      t.HasCheckConstraint("CK_DiagramConnections_Type",
        "[ConnectionType] BETWEEN 0 AND 3");

      t.HasCheckConstraint("CK_DiagramConnections_Elements",
        "[SourceElementId] <> [TargetElementId]");
    });

    // Ignore domain events
    builder.Ignore(c => c.DomainEvents);
  }
}
