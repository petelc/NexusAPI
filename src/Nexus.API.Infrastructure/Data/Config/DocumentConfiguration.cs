using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Enums;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for Document aggregate root
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents", "dbo");

        // Primary Key
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DocumentId.From(value))
            .ValueGeneratedNever();

        // Value Objects - Title
        builder.OwnsOne(d => d.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("Title")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Value Objects - Content
        builder.OwnsOne(d => d.Content, content =>
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
        builder.Property(d => d.CreatedBy)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();

        builder.Property(d => d.LastEditedBy);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.ReadingTimeMinutes)
            .IsRequired();

        builder.Property(d => d.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(d => d.IsDeleted)
            .IsRequired();

        builder.Property(d => d.DeletedAt);

        // Relationships - Tags (Many-to-Many)
        builder.HasMany(d => d.Tags)
            .WithMany()
            .UsingEntity(
                "DocumentTags",
                l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(Document)).WithMany().HasForeignKey("DocumentId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("DocumentTags", "dbo");
                    j.Property<DateTime>("AddedAt").HasDefaultValueSql("GETUTCDATE()");
                    j.Property<Guid>("AddedBy");
                });

        // Relationships - Versions (One-to-Many)
        builder.HasMany(d => d.Versions)
            .WithOne()
            .HasForeignKey("DocumentId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.CreatedBy)
            .HasDatabaseName("IX_Documents_CreatedBy");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("IX_Documents_Status")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(d => d.CreatedAt)
            .HasDatabaseName("IX_Documents_CreatedAt");

        builder.HasIndex(d => d.UpdatedAt)
            .HasDatabaseName("IX_Documents_UpdatedAt");

        // Ignore Domain Events
        builder.Ignore(d => d.DomainEvents);
    }
}
