using Ardalis.GuardClauses;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.DocumentAggregate;

/// <summary>
/// Represents a version snapshot of a document
/// </summary>
public class DocumentVersion : EntityBase<Guid>
{
    public DocumentId DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public DocumentContent Content { get; private set; } = null!;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string ChangeDescription { get; private set; } = string.Empty;
    public string? ContentHash { get; private set; }

    // Private constructor for EF Core
    private DocumentVersion() { }

    /// <summary>
    /// Factory method to create a new document version
    /// </summary>
    public static DocumentVersion Create(
        DocumentId documentId,
        int versionNumber,
        DocumentContent content,
        Guid createdBy,
        string changeDescription)
    {
        Guard.Against.Null(documentId, nameof(documentId));
        Guard.Against.NegativeOrZero(versionNumber, nameof(versionNumber));
        Guard.Against.Null(content, nameof(content));
        Guard.Against.Default(createdBy, nameof(createdBy));

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = versionNumber,
            Content = content,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            ChangeDescription = changeDescription ?? string.Empty,
            ContentHash = ComputeHash(content.RichText)
        };

        return version;
    }

    /// <summary>
    /// Compute SHA256 hash of content for deduplication
    /// </summary>
    private static string ComputeHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }
}
