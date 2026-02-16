using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users", "dbo");

    // Primary key
    builder.HasKey(u => u.Id);

    // Configure UserId as value object
    builder.Property(u => u.Id)
      .HasConversion(
        id => id.Value,
        value => UserId.From(value))
      .IsRequired();

    // Configure Email as value object
    builder.OwnsOne(u => u.Email, email =>
    {
      email.Property(e => e.Address)
        .HasColumnName("Email")
        .HasMaxLength(256)
        .IsRequired();

      // Index is configured here inside the owned type configuration
      email.HasIndex(e => e.Address)
        .IsUnique()
        .HasDatabaseName("IX_Users_Email");
    });

    // Username
    builder.Property(u => u.Username)
      .HasMaxLength(100)
      .IsRequired();

    builder.HasIndex(u => u.Username)
      .IsUnique();

    // Configure PersonName as value object
    builder.OwnsOne(u => u.FullName, fullName =>
    {
      fullName.Property(n => n.FirstName)
        .HasColumnName("FirstName")
        .HasMaxLength(100)
        .IsRequired();

      fullName.Property(n => n.LastName)
        .HasColumnName("LastName")
        .HasMaxLength(100)
        .IsRequired();
    });

    // Password hash
    builder.Property(u => u.PasswordHash)
      .HasMaxLength(256)
      .IsRequired();

    // Flags
    builder.Property(u => u.EmailConfirmed)
      .IsRequired();

    builder.Property(u => u.TwoFactorEnabled)
      .IsRequired();

    builder.Property(u => u.IsActive)
      .IsRequired();

    // Timestamps
    builder.Property(u => u.CreatedAt)
      .IsRequired();

    builder.Property(u => u.LastLoginAt);

    // Configure UserProfile as value object
    builder.OwnsOne(u => u.Profile, profile =>
    {
      profile.Property(p => p.AvatarUrl)
        .HasColumnName("AvatarUrl")
        .HasMaxLength(500);

      profile.Property(p => p.Bio)
        .HasColumnName("Bio")
        .HasMaxLength(1000);

      profile.Property(p => p.Title)
        .HasColumnName("Title")
        .HasMaxLength(100);

      profile.Property(p => p.Department)
        .HasColumnName("Department")
        .HasMaxLength(100);
    });

    // Configure UserPreferences as value object
    builder.OwnsOne(u => u.Preferences, preferences =>
    {
      preferences.Property(p => p.Theme)
        .HasColumnName("Theme")
        .HasConversion<int>()
        .IsRequired();

      preferences.Property(p => p.Language)
        .HasColumnName("Language")
        .HasMaxLength(10)
        .IsRequired();

      preferences.Property(p => p.NotificationsEnabled)
        .HasColumnName("NotificationsEnabled")
        .IsRequired();

      preferences.Property(p => p.EmailDigest)
        .HasColumnName("EmailDigest")
        .HasConversion<int>()
        .IsRequired();
    });

    // Other indexes (NOT Email - that's configured in the OwnsOne block above)
    builder.HasIndex(u => u.CreatedAt)
      .HasDatabaseName("IX_Users_CreatedAt");

    builder.HasIndex(u => u.IsActive)
      .HasDatabaseName("IX_Users_IsActive");

    // Ignore domain events (handled by base class)
    builder.Ignore(u => u.DomainEvents);
  }
}
