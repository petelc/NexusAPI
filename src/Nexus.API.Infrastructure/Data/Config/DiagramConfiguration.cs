using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Diagram aggregate root
/// </summary>
public class DiagramConfiguration : IEntityTypeConfiguration<Diagram>
{
  public void Configure(EntityTypeBuilder<Diagram> builder)
  {
    builder.ToTable("Diagrams", "dbo");

    // Primary Key
    builder.HasKey(d => d.Id);

    builder.Property(d => d.Id)
      .HasConversion(
        id => id.Value,
        value => DiagramId.Create(value))
      .HasColumnName("DiagramId")
      .IsRequired();

    // Title (Value Object)
    builder.OwnsOne(d => d.Title, title =>
    {
      title.Property(t => t.Value)
        .HasColumnName("Title")
        .HasMaxLength(200)
        .IsRequired();
    });

    // DiagramType (Enum)
    builder.Property(d => d.DiagramType)
      .HasConversion<int>()
      .IsRequired();

    // Canvas (Value Object)
    builder.OwnsOne(d => d.Canvas, canvas =>
    {
      canvas.Property(c => c.Width)
        .HasColumnName("CanvasWidth")
        .HasColumnType("decimal(10,2)")
        .IsRequired();

      canvas.Property(c => c.Height)
        .HasColumnName("CanvasHeight")
        .HasColumnType("decimal(10,2)")
        .IsRequired();

      canvas.Property(c => c.BackgroundColor)
        .HasColumnName("BackgroundColor")
        .HasMaxLength(7)
        .IsRequired();

      canvas.Property(c => c.GridSize)
        .HasColumnName("GridSize")
        .IsRequired(false);
    });

    // Timestamps
    builder.Property(d => d.CreatedBy)
      .IsRequired();

    builder.Property(d => d.CreatedAt)
      .IsRequired()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.Property(d => d.UpdatedAt)
      .IsRequired()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    // Soft Delete
    builder.Property(d => d.IsDeleted)
      .IsRequired()
      .HasDefaultValue(false);

    builder.Property(d => d.DeletedAt)
      .IsRequired(false);

    // Relationships - Elements
    builder.HasMany<DiagramElement>("_elements")
      .WithOne()
      .HasForeignKey("DiagramId")
      .OnDelete(DeleteBehavior.Cascade);

    // Relationships - Connections
    builder.HasMany<DiagramConnection>("_connections")
      .WithOne()
      .HasForeignKey("DiagramId")
      .HasPrincipalKey(d => d.Id)  // Reference the property, not the column
      .OnDelete(DeleteBehavior.Cascade);

    // Relationships - Layers
    builder.HasMany<Layer>("_layers")
      .WithOne()
      .HasForeignKey("DiagramId") // Reference the property, not the column
      .HasPrincipalKey(d => d.Id)  // Reference the property, not the column
      .OnDelete(DeleteBehavior.Cascade);

    // Tell EF Core to ignore the public properties
    builder.Ignore(d => d.Elements);
    builder.Ignore(d => d.Connections);
    builder.Ignore(d => d.Layers);

    // Indexes
    builder.HasIndex(d => d.CreatedBy)
      .HasDatabaseName("IX_Diagrams_CreatedBy");

    builder.HasIndex(d => d.DiagramType)
      .HasDatabaseName("IX_Diagrams_DiagramType")
      .HasFilter("[IsDeleted] = 0");

    builder.HasIndex(d => d.CreatedAt)
      .HasDatabaseName("IX_Diagrams_CreatedAt")
      .IsDescending();

    builder.HasIndex(d => d.UpdatedAt)
      .HasDatabaseName("IX_Diagrams_UpdatedAt")
      .IsDescending();

    builder.HasIndex(d => d.IsDeleted)
      .HasDatabaseName("IX_Diagrams_IsDeleted");

    // Query Filter for soft deletes
    builder.HasQueryFilter(d => !d.IsDeleted);

    // Ignore domain events collection
    builder.Ignore(d => d.DomainEvents);
  }
}
