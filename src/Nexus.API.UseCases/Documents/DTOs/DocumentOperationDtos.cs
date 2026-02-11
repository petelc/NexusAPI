namespace Nexus.API.UseCases.Documents.DTOs;

// ── Request DTOs ──────────────────────────────────────────────────────────────

/// <summary>
/// PUT /api/v1/documents/{id} — update title, content, and/or status
/// </summary>
public record UpdateDocumentRequest(
    string? Title,
    string? Content,
    string? Status,
    string? LanguageCode);

/// <summary>
/// POST /api/v1/documents/{id}/tags — add one or more tags
/// </summary>
public record AddDocumentTagsRequest(
    List<string> Tags);

// ── Response DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Minimal response after update — callers can GET the full document if needed.
/// </summary>
public record UpdateDocumentResponse(
    Guid DocumentId,
    string Title,
    string Status,
    DateTime UpdatedAt);

/// <summary>
/// Summary of a single document version, used in version list responses.
/// </summary>
public record DocumentVersionSummaryDto(
    Guid VersionId,
    int VersionNumber,
    Guid CreatedBy,
    DateTime CreatedAt,
    string ChangeDescription);

/// <summary>
/// Full version detail including content, returned by GET /versions/{n}.
/// </summary>
public record DocumentVersionDetailDto(
    Guid VersionId,
    Guid DocumentId,
    int VersionNumber,
    string ContentRichText,
    string ContentPlainText,
    Guid CreatedBy,
    DateTime CreatedAt,
    string ChangeDescription);
