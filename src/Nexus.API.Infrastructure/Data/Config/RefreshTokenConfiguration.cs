using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Entities;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for RefreshToken entity
/// Configures table, indexes, and constraints
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder.ToTable("RefreshTokens", "identity");

    builder.HasKey(rt => rt.Id);

    builder.Property(rt => rt.Id)
      .IsRequired();

    builder.Property(rt => rt.UserId)
      .IsRequired();

    builder.Property(rt => rt.Token)
      .IsRequired()
      .HasMaxLength(256);

    builder.Property(rt => rt.JwtId)
      .IsRequired()
      .HasMaxLength(256);

    builder.Property(rt => rt.CreatedAt)
      .IsRequired();

    builder.Property(rt => rt.ExpiresAt)
      .IsRequired();

    builder.Property(rt => rt.Used)
      .IsRequired();

    builder.Property(rt => rt.UsedAt)
      .IsRequired(false);

    builder.Property(rt => rt.Invalidated)
      .IsRequired();

    builder.Property(rt => rt.InvalidatedAt)
      .IsRequired(false);

    builder.Property(rt => rt.InvalidatedReason)
      .HasMaxLength(500)
      .IsRequired(false);

    // Indexes
    builder.HasIndex(rt => rt.Token)
      .IsUnique()
      .HasDatabaseName("IX_RefreshTokens_Token");

    builder.HasIndex(rt => rt.JwtId)
      .IsUnique()
      .HasDatabaseName("IX_RefreshTokens_JwtId");

    builder.HasIndex(rt => rt.UserId)
      .HasDatabaseName("IX_RefreshTokens_UserId");

    builder.HasIndex(rt => rt.ExpiresAt)
      .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

    // Composite index for common queries
    builder.HasIndex(rt => new { rt.UserId, rt.Used, rt.Invalidated, rt.ExpiresAt })
      .HasDatabaseName("IX_RefreshTokens_UserValidation");
  }
}
