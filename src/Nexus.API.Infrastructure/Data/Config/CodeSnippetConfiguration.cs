using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.API.Core.Aggregates.CodeSnippetAggregate;
using Nexus.API.Core.Aggregates.DocumentAggregate;

namespace Nexus.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for CodeSnippet aggregate
/// Configures table, indexes, relationships, and value objects
/// </summary>
public class CodeSnippetConfiguration : IEntityTypeConfiguration<CodeSnippet>
{
  public void Configure(EntityTypeBuilder<CodeSnippet> builder)
  {
    builder.ToTable("CodeSnippets", "dbo");

    builder.HasKey(cs => cs.Id);

    builder.Property(cs => cs.Id)
      .IsRequired();

    // Configure Title value object as owned type
    builder.OwnsOne(cs => cs.Title, title =>
    {
      title.Property(t => t.Value)
        .HasColumnName("Title")
        .HasMaxLength(200)
        .IsRequired();
    });

    builder.Property(cs => cs.Code)
      .IsRequired();

    // Configure ProgrammingLanguage value object as owned type
    builder.OwnsOne(cs => cs.Language, lang =>
    {
      lang.Property(l => l.Name)
        .HasColumnName("LanguageName")
        .HasMaxLength(50)
        .IsRequired();

      lang.Property(l => l.FileExtension)
        .HasColumnName("FileExtension")
        .HasMaxLength(10)
        .IsRequired();

      lang.Property(l => l.Version)
        .HasColumnName("LanguageVersion")
        .HasMaxLength(20)
        .IsRequired(false);
    });

    builder.Property(cs => cs.Description)
      .HasMaxLength(1000)
      .IsRequired(false);

    builder.Property(cs => cs.CreatedBy)
      .IsRequired();

    builder.Property(cs => cs.CreatedAt)
      .IsRequired();

    builder.Property(cs => cs.UpdatedAt)
      .IsRequired();

    builder.Property(cs => cs.OriginalSnippetId)
      .IsRequired(false);

    builder.Property(cs => cs.IsDeleted)
      .IsRequired();

    builder.Property(cs => cs.DeletedAt)
      .IsRequired(false);

    // Configure SnippetMetadata value object as owned type
    builder.OwnsOne(cs => cs.Metadata, metadata =>
    {
      metadata.Property(m => m.LineCount)
        .HasColumnName("LineCount")
        .IsRequired();

      metadata.Property(m => m.CharacterCount)
        .HasColumnName("CharacterCount")
        .IsRequired();

      metadata.Property(m => m.IsPublic)
        .HasColumnName("IsPublic")
        .IsRequired();

      metadata.Property(m => m.ForkCount)
        .HasColumnName("ForkCount")
        .IsRequired();

      metadata.Property(m => m.ViewCount)
        .HasColumnName("ViewCount")
        .IsRequired();

      metadata.HasIndex(cs => new { cs.IsPublic })
      .HasDatabaseName("IX_CodeSnippets_PublicDeleted");
    });

    // Configure Tags many-to-many relationship
    builder.HasMany(cs => cs.Tags)
      .WithMany()
      .UsingEntity<Dictionary<string, object>>(
        "CodeSnippetTags",
        j => j.HasOne<Tag>()
          .WithMany()
          .HasForeignKey("TagId")
          .OnDelete(DeleteBehavior.Cascade),
        j => j.HasOne<CodeSnippet>()
          .WithMany()
          .HasForeignKey("CodeSnippetId")
          .OnDelete(DeleteBehavior.Cascade),
        j =>
        {
          j.ToTable("CodeSnippetTags", "dbo");
          j.HasKey("CodeSnippetId", "TagId");
          j.Property<DateTime>("AddedAt").HasDefaultValueSql("GETUTCDATE()");
        });

    // Configure Forks as owned collection
    builder.OwnsMany(cs => cs.Forks, fork =>
    {
      fork.ToTable("SnippetForks", "dbo");
      fork.WithOwner().HasForeignKey("OriginalSnippetId");
      fork.HasKey(nameof(SnippetFork.OriginalSnippetId), nameof(SnippetFork.ForkedSnippetId));

      fork.Property(f => f.OriginalSnippetId).IsRequired();
      fork.Property(f => f.ForkedSnippetId).IsRequired();
      fork.Property(f => f.ForkedBy).IsRequired();
      fork.Property(f => f.ForkedAt).IsRequired();
    });

    // Indexes
    builder.HasIndex(cs => cs.CreatedBy)
      .HasDatabaseName("IX_CodeSnippets_CreatedBy");



    builder.HasIndex(cs => cs.CreatedAt)
      .HasDatabaseName("IX_CodeSnippets_CreatedAt");

    builder.HasIndex(cs => cs.OriginalSnippetId)
      .HasDatabaseName("IX_CodeSnippets_OriginalSnippetId")
      .HasFilter("[OriginalSnippetId] IS NOT NULL");

    // Ignore domain events
    builder.Ignore(cs => cs.DomainEvents);
  }
}
