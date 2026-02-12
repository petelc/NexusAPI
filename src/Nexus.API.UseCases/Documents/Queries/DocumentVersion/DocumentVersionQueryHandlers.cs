using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Documents.DTOs;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.UseCases.Documents.Commands.UpdateDocument;

namespace Nexus.API.UseCases.Documents.Queries;

// ============================================================
// ListDocumentVersions
// ============================================================

// --- Query ---

public record ListDocumentVersionsQuery(
    Guid DocumentId,
    Guid UserId) : IRequest<Result<IReadOnlyList<DocumentVersionSummaryDto>>>;

// --- Handler ---

public class ListDocumentVersionsQueryHandler : IRequestHandler<ListDocumentVersionsQuery, Result<IReadOnlyList<DocumentVersionSummaryDto>>>
{
    private readonly IDocumentRepository _documentRepository;

    public ListDocumentVersionsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result<IReadOnlyList<DocumentVersionSummaryDto>>> Handle(
        ListDocumentVersionsQuery query,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(query.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result<IReadOnlyList<DocumentVersionSummaryDto>>.NotFound("Document not found.");

        // Owner or any user with access may view version history
        if (document.CreatedBy != query.UserId)
            return Result<IReadOnlyList<DocumentVersionSummaryDto>>.Unauthorized();

        var versions = document.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionSummaryDto(
                v.Id,
                v.VersionNumber,
                v.CreatedBy,
                v.CreatedAt,
                v.ChangeDescription))
            .ToList();

        return Result<IReadOnlyList<DocumentVersionSummaryDto>>.Success(versions);
    }
}

// ============================================================
// GetDocumentVersion
// ============================================================

// --- Query ---

public record GetDocumentVersionQuery(
    Guid DocumentId,
    int VersionNumber,
    Guid UserId) : IRequest<Result<DocumentVersionDetailDto>>;

// --- Handler ---

public class GetDocumentVersionQueryHandler : IRequestHandler<GetDocumentVersionQuery, Result<DocumentVersionDetailDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentVersionQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result<DocumentVersionDetailDto>> Handle(
        GetDocumentVersionQuery query,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(query.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result<DocumentVersionDetailDto>.NotFound("Document not found.");

        if (document.CreatedBy != query.UserId)
            return Result<DocumentVersionDetailDto>.Unauthorized();

        var version = document.Versions
            .FirstOrDefault(v => v.VersionNumber == query.VersionNumber);

        if (version is null)
            return Result<DocumentVersionDetailDto>.NotFound(
                $"Version {query.VersionNumber} not found for this document.");

        return Result<DocumentVersionDetailDto>.Success(
            new DocumentVersionDetailDto(
                version.Id,
                document.Id.Value,
                version.VersionNumber,
                version.Content.RichText,
                version.Content.PlainText,
                version.CreatedBy,
                version.CreatedAt,
                version.ChangeDescription));
    }
}

// ============================================================
// RestoreDocumentVersion
// ============================================================

// --- Command (lives here for locality — restore is query-shaped but mutates) ---

public record RestoreDocumentVersionCommand(
    Guid DocumentId,
    int VersionNumber,
    Guid UserId) : IRequest<Result<UpdateDocumentResponse>>;

// --- Handler ---

public class RestoreDocumentVersionCommandHandler : IRequestHandler<RestoreDocumentVersionCommand, Result<UpdateDocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public RestoreDocumentVersionCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result<UpdateDocumentResponse>> Handle(
        RestoreDocumentVersionCommand command,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(command.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result<UpdateDocumentResponse>.NotFound("Document not found.");

        if (document.CreatedBy != command.UserId)
            return Result<UpdateDocumentResponse>.Unauthorized();

        var version = document.Versions
            .FirstOrDefault(v => v.VersionNumber == command.VersionNumber);

        if (version is null)
            return Result<UpdateDocumentResponse>.NotFound(
                $"Version {command.VersionNumber} not found for this document.");

        try
        {
            // Restoring applies the version's content as a new content update,
            // which internally snapshots the current state as a new version first.
            var restoredContent = DocumentContent.Create(version.Content.RichText);
            document.UpdateContent(restoredContent, command.UserId);

            await _documentRepository.UpdateAsync(document, cancellationToken);

            return Result<UpdateDocumentResponse>.Success(
                new UpdateDocumentResponse(
                    document.Id.Value,
                    document.Title.Value,
                    document.Status.ToString(),
                    document.UpdatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Result<UpdateDocumentResponse>.Invalid(
                new ValidationError { ErrorMessage = ex.Message });
        }
    }
}
