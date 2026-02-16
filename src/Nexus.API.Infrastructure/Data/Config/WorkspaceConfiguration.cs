using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Workspace aggregate
/// </summary>
public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
  public void Configure(EntityTypeBuilder<Workspace> builder)
  {
    builder.ToTable("Workspaces", "dbo");

    // Primary Key
    builder.HasKey(w => w.Id);

    builder.Property(w => w.Id)
      .HasConversion(
        id => id.Value,
        value => WorkspaceId.Create(value))
      .HasColumnName("Id")
      .IsRequired();

    // Properties
    builder.Property(w => w.Name)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(w => w.Description)
      .HasMaxLength(1000);

    builder.Property(w => w.TeamId)
      .HasConversion(
        id => id.Value,
        value => TeamId.Create(value))
      .IsRequired();

    builder.Property(w => w.CreatedBy)
      .HasConversion(
        id => id.Value,
        value => UserId.From(value))
      .IsRequired();

    builder.Property(w => w.CreatedAt)
      .IsRequired();

    builder.Property(w => w.UpdatedAt)
      .IsRequired();

    builder.Property(w => w.IsDeleted)
      .IsRequired()
      .HasDefaultValue(false);

    builder.Property(w => w.DeletedAt);

    // Owned Collection: Members
    builder.OwnsMany(w => w.Members, mb =>
    {
      mb.ToTable("WorkspaceMembers", "dbo");

      mb.WithOwner().HasForeignKey("WorkspaceId");

      mb.HasKey(nameof(WorkspaceMember.Id), "WorkspaceId");

      mb.Property(m => m.Id)
        .HasConversion(
          id => id.Value,
          value => WorkspaceMemberId.Create(value))
        .HasColumnName("Id")
        .IsRequired();

      mb.Property(m => m.UserId)
        .HasConversion(
          id => id.Value,
          value => UserId.From(value))
        .IsRequired();

      mb.Property(m => m.Role)
        .HasConversion<int>()
        .IsRequired();

      mb.Property(m => m.InvitedBy)
        .HasConversion(
          id => id != null ? id.Value : (Guid?)null,
          value => value.HasValue ? UserId.From(value.Value) : null);

      mb.Property(m => m.JoinedAt)
        .IsRequired();

      mb.Property(m => m.IsActive)
        .IsRequired()
        .HasDefaultValue(true);

      // Indexes
      mb.HasIndex(m => m.UserId);
      mb.HasIndex(m => m.IsActive);
    });

    // Indexes
    builder.HasIndex(w => w.TeamId);
    builder.HasIndex(w => w.CreatedBy);
    builder.HasIndex(w => w.IsDeleted);
    builder.HasIndex(w => new { w.Name, w.TeamId });

    // Ignore domain events collection
    builder.Ignore(w => w.DomainEvents);
  }
}
