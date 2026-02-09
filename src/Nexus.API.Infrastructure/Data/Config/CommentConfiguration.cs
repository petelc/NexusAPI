using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollaborationAggregate;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for Comment
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Table mapping
        builder.ToTable("Comments", "collaboration");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("CommentId")
            .ValueGeneratedNever(); // Generated in domain

        // Properties
        builder.Property(e => e.SessionId);

        builder.Property(e => e.ResourceType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ResourceId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Position);

        builder.Property(e => e.ParentCommentId);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt)
            .HasColumnType("datetime2(7)");

        // Relationships
        builder.HasOne(e => e.Session)
            .WithMany(s => s.Comments)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(e => e.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade cycles

        // Indexes
        builder.HasIndex(e => new { e.ResourceType, e.ResourceId })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Comments_Resource");

        builder.HasIndex(e => e.SessionId)
            .HasDatabaseName("IX_Comments_Session");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Comments_UserId");

        builder.HasIndex(e => e.ParentCommentId)
            .HasDatabaseName("IX_Comments_ParentComment");
    }
}
