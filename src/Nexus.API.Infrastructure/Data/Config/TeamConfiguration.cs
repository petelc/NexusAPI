using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Team aggregate
/// </summary>
/// 
/// <remarks>
/// This configuration class defines how the Team aggregate is mapped to the database using Entity Framework Core. It specifies the table name, primary key, property configurations, and relationships with other entities. The configuration ensures that the Team entity is properly stored and retrieved from the database while adhering to the domain model's constraints and requirements.
/// </remarks>
public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        // Primary Key
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TeamId.Create(value))
            .HasColumnName("TeamId")
            .IsRequired();

        // Properties
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Workspaces)
            .WithOne()
            .HasForeignKey(w => w.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}