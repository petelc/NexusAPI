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


