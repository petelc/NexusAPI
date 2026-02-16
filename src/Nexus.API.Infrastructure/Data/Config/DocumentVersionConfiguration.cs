using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DocumentAggregate;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for DocumentVersion entity
/// </summary>
public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions", "dbo");

        // Primary Key — client-generated Guid
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        // DocumentId (Foreign Key)
        builder.Property<DocumentId>("DocumentId")
            .HasConversion(
                id => id.Value,
                value => new DocumentId(value))
            .IsRequired();

        // Value Objects - Content
        builder.OwnsOne(v => v.Content, content =>
        {
            content.Property(c => c.RichText)
                .HasColumnName("ContentRichText")
                .IsRequired();

            content.Property(c => c.PlainText)
                .HasColumnName("ContentPlainText")
                .IsRequired();

            content.Property(c => c.WordCount)
                .HasColumnName("WordCount")
                .IsRequired();
        });

        // Scalar Properties
        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.CreatedBy)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.ChangeDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(v => v.ContentHash)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex("DocumentId", "VersionNumber")
            .HasDatabaseName("IX_DocumentVersions_DocumentId_VersionNumber");

        builder.HasIndex(v => v.CreatedAt)
            .HasDatabaseName("IX_DocumentVersions_CreatedAt");

        // Unique Constraint
        builder.HasIndex("DocumentId", "VersionNumber")
            .IsUnique()
            .HasDatabaseName("UQ_DocumentVersions_DocumentVersion");

        // Ignore Domain Events
        builder.Ignore(v => v.DomainEvents);
    }
}
