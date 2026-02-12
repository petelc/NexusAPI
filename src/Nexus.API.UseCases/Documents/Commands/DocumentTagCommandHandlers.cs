using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Commands;

// ============================================================
// AddDocumentTags
// ============================================================

// --- Command ---

public record AddDocumentTagsCommand(
    Guid DocumentId,
    Guid UserId,
    IReadOnlyList<string> TagNames) : IRequest<Result>;

// --- Handler ---

public class AddDocumentTagsCommandHandler : IRequestHandler<AddDocumentTagsCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITagRepository _tagRepository;

    public AddDocumentTagsCommandHandler(
        IDocumentRepository documentRepository,
        ITagRepository tagRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
        _tagRepository = tagRepository
            ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    public async Task<Result> Handle(
        AddDocumentTagsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TagNames == null || command.TagNames.Count == 0)
            return Result.Invalid(new ValidationError { ErrorMessage = "At least one tag name is required." });

        var documentId = new DocumentId(command.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result.NotFound("Document not found.");

        if (document.CreatedBy != command.UserId)
            return Result.Unauthorized();

        foreach (var tagName in command.TagNames)
        {
            var tag = await _tagRepository.GetOrCreateByNameAsync(tagName, cancellationToken);
            document.AddTag(tag);
        }

        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}

// ============================================================
// RemoveDocumentTag
// ============================================================

// --- Command ---

public record RemoveDocumentTagCommand(
    Guid DocumentId,
    Guid UserId,
    string TagName) : IRequest<Result>;

// --- Handler ---

public class RemoveDocumentTagCommandHandler : IRequestHandler<RemoveDocumentTagCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITagRepository _tagRepository;

    public RemoveDocumentTagCommandHandler(
        IDocumentRepository documentRepository,
        ITagRepository tagRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
        _tagRepository = tagRepository
            ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    public async Task<Result> Handle(
        RemoveDocumentTagCommand command,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(command.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result.NotFound("Document not found.");

        if (document.CreatedBy != command.UserId)
            return Result.Unauthorized();

        var tag = document.Tags.FirstOrDefault(
            t => t.Name.Equals(command.TagName, StringComparison.OrdinalIgnoreCase));

        if (tag is null)
            return Result.NotFound($"Tag '{command.TagName}' not found on this document.");

        document.RemoveTag(tag);
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
