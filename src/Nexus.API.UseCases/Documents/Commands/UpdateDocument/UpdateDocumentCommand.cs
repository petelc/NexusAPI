using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.Core.Aggregates.DocumentAggregate;
using Nexus.Core.Interfaces;
using Nexus.Core.ValueObjects;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Commands.UpdateDocument;

/// <summary>
/// Command to update an existing document
/// </summary>
public record UpdateDocumentCommand : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public Guid UpdatedBy { get; init; }
}

/// <summary>
/// Validator for UpdateDocumentCommand
/// </summary>
public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId is required");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required");

        RuleFor(x => x.Title)
            .MinimumLength(1).WithMessage("Title must be at least 1 character")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content cannot be empty")
            .When(x => x.Content != null);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Title) || !string.IsNullOrEmpty(x.Content))
            .WithMessage("At least one field (Title or Content) must be provided for update");
    }
}

/// <summary>
/// Handler for UpdateDocumentCommand
/// </summary>
public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<DocumentDto> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get the document
        var documentId = DocumentId.From(request.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document == null)
            throw new InvalidOperationException($"Document with ID {request.DocumentId} not found");

        // Update content if provided
        if (!string.IsNullOrEmpty(request.Content))
        {
            var newContent = DocumentContent.Create(request.Content);
            document.UpdateContent(newContent, request.UpdatedBy);
        }

        // Update title if provided (would need to add this method to Document aggregate)
        // For now, we'll need to update via content update

        // Save changes
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Map to DTO and return
        return _mapper.Map<DocumentDto>(document);
    }
}
