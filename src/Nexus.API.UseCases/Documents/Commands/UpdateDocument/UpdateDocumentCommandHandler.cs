using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Documents.Commands.UpdateDocument;
using Nexus.API.UseCases.Documents.DTOs;

namespace Nexus.API.UseCases.Documents.Commands.UpdateDocument;


public sealed class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, Result<UpdateDocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public UpdateDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository
            ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result<UpdateDocumentResponse>> Handle(
        UpdateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var documentId = new DocumentId(command.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.IsDeleted)
            return Result.NotFound("Document not found.");

        // Ownership or explicit permission check — owners are always the CreatedBy user.
        // Phase B's IPermissionService wires finer-grained checks; for now we check ownership.
        if (document.CreatedBy != command.UpdatedBy)
            return Result.Unauthorized();

        try
        {
            // Apply title update
            if (!string.IsNullOrWhiteSpace(command.Title))
            {
                var newTitle = Title.Create(command.Title);
                document.UpdateTitle(newTitle, command.UpdatedBy);
            }

            // Apply content update (also snapshots a version internally)
            if (!string.IsNullOrWhiteSpace(command.Content))
            {
                var newContent = DocumentContent.Create(command.Content);
                document.UpdateContent(newContent, command.UpdatedBy);
            }

            // Apply status transition
            if (!string.IsNullOrWhiteSpace(command.Status))
            {
                switch (command.Status.ToLowerInvariant())
                {
                    case "published":
                        document.Publish(command.UpdatedBy);
                        break;
                    case "archived":
                        document.Archive(command.UpdatedBy);
                        break;
                    case "draft":
                        // No explicit "revert to draft" method on the aggregate;
                        // guard and do nothing if already draft
                        break;
                    default:
                        return Result.Invalid(
                            new ValidationError
                            {
                                ErrorMessage = $"Invalid status '{command.Status}'. Valid values: draft, published, archived."
                            });
                }
            }

            await _documentRepository.SaveChangesAsync(cancellationToken);

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
