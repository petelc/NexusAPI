using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Team aggregate
/// Maps Team and its owned TeamMember collection to database tables
/// </summary>
public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams", "dbo");

        // Primary Key
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TeamId.Create(value))
            .HasColumnName("Id")
            .IsRequired();

        // Properties
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedBy)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt);

        // Owned Entity - TeamMembers
        builder.OwnsMany(t => t.Members, membersBuilder =>
        {
            membersBuilder.ToTable("TeamMembers", "dbo");

            // Composite Primary Key
            membersBuilder.WithOwner()
                .HasForeignKey(nameof(TeamMember.TeamId));

            membersBuilder.HasKey(nameof(TeamMember.Id), nameof(TeamMember.TeamId));

            membersBuilder.Property(m => m.Id)
                .HasConversion(
                    id => id.Value,
                    value => TeamMemberId.Create(value))
                .HasColumnName("Id")
                .IsRequired();

            membersBuilder.Property(m => m.TeamId)
                .HasConversion(
                    id => id.Value,
                    value => TeamId.Create(value))
                .IsRequired();

            membersBuilder.Property(m => m.UserId)
                .IsRequired();

            membersBuilder.Property(m => m.Role)
                .HasConversion<int>()
                .IsRequired();

            membersBuilder.Property(m => m.InvitedBy);

            membersBuilder.Property(m => m.JoinedAt)
                .IsRequired();

            membersBuilder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes for TeamMembers
            membersBuilder.HasIndex(m => m.UserId)
                .HasDatabaseName("IX_TeamMembers_UserId");

            membersBuilder.HasIndex(m => new { m.TeamId, m.UserId })
                .HasDatabaseName("IX_TeamMembers_TeamId_UserId");
        });

        // Indexes
        builder.HasIndex(t => t.Name)
            .HasDatabaseName("IX_Teams_Name");

        builder.HasIndex(t => t.CreatedBy)
            .HasDatabaseName("IX_Teams_CreatedBy");

        builder.HasIndex(t => t.IsDeleted)
            .HasDatabaseName("IX_Teams_IsDeleted");

        // Query Filter for soft deletes
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
