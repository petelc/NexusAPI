using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;

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
            .HasConversion(
                id => id.Value,
                value => CommentId.Create(value))
            .HasColumnName("Id")
            .ValueGeneratedNever(); // Generated in domain

        // Map CLR Guid? SessionId as a regular column (not used as FK)
        builder.Property(e => e.SessionId)
            .HasColumnName("SessionId");

        // Shadow FK for the relationship - must be SessionId? to match the PK type
        builder.Property<SessionId?>("CollaborationSessionId")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? SessionId.Create(value.Value) : null)
            .HasColumnName("CollaborationSessionId");

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

        // Map CLR Guid? ParentCommentId as a regular column (not used as FK)
        builder.Property(e => e.ParentCommentId)
            .HasColumnName("ParentCommentId");

        // Shadow FK for ParentComment relationship - must be CommentId? to match the PK type
        builder.Property<CommentId?>("ParentCommentFk")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? CommentId.Create(value.Value) : null)
            .HasColumnName("ParentCommentFkId");

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

        // Relationships - use shadow property for FK since CLR SessionId is Guid? but PK is SessionId
        builder.HasOne(e => e.Session)
            .WithMany(s => s.Comments)
            .HasForeignKey("CollaborationSessionId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey("ParentCommentFk")
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade cycles

        // Indexes
        builder.HasIndex(e => new { e.ResourceType, e.ResourceId })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Comments_Resource");

        builder.HasIndex("CollaborationSessionId")
            .HasDatabaseName("IX_Comments_Session");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Comments_UserId");

        builder.HasIndex("ParentCommentFk")
            .HasDatabaseName("IX_Comments_ParentComment");
    }
}
