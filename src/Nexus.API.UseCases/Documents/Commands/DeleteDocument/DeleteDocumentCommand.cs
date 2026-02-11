using FluentValidation;
using MediatR;
using Ardalis.Result;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Documents.Commands.DeleteDocument;

/// <summary>
/// Command to delete (soft delete) a document
/// </summary>
public record DeleteDocumentCommand : IRequest<Result>
{
    public Guid DocumentId { get; init; }
    public Guid DeletedBy { get; init; }
    public bool Permanent { get; init; } = false;
}

/// <summary>
/// Validator for DeleteDocumentCommand
/// </summary>
public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId is required");

        RuleFor(x => x.DeletedBy)
            .NotEmpty().WithMessage("DeletedBy is required");
    }
}

/// <summary>
/// Handler for DeleteDocumentCommand
/// </summary>
public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;

    public DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get the document
        var documentId = DocumentId.From(request.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document == null)
            return false;

        if (request.Permanent)
        {
            // Permanent delete
            await _documentRepository.DeleteAsync(document, cancellationToken);
        }
        else
        {
            // Soft delete
            document.Delete(request.DeletedBy);
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }

        return true;
    }
}
