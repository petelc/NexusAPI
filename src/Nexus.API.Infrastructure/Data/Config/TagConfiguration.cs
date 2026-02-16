using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for Tag entity
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags", "dbo");

        // Primary Key
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("TagId")
            .HasConversion(id => id.Value, value => TagId.Create(value))
            .IsRequired();

        // Scalar Properties
        builder.Property(t => t.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Color)
            .HasMaxLength(7); // #RRGGBB format

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Unique Constraint
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Tags_Name");

        // Ignore Domain Events
        builder.Ignore(t => t.DomainEvents);
    }
}
