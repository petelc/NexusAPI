using Ardalis.Result;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Commands.DeleteDocument;

// --- Command ---

// public record DeleteDocumentCommand(
//     Guid DocumentId,
//     Guid UserId,
//     bool Permanent = false);

// --- Handler ---

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;

    public DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result> Handle(
        DeleteDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(command.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result.NotFound("Document not found.");

        if (document.CreatedBy != command.DeletedBy)
            return Result.Unauthorized();

        if (command.Permanent)
        {
            // Hard delete — removes the row from the database
            await _documentRepository.DeleteAsync(document, cancellationToken);
        }
        else
        {
            // Soft delete — sets IsDeleted = true, preserves the record
            document.Delete(command.DeletedBy);
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }

        return Result.Success();
    }
}
