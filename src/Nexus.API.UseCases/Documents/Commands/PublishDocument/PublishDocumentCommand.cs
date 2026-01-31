using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.Core.Aggregates.DocumentAggregate;
using Nexus.Core.Interfaces;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Commands.PublishDocument;

/// <summary>
/// Command to publish a document
/// </summary>
public record PublishDocumentCommand : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
    public Guid PublishedBy { get; init; }
}

/// <summary>
/// Validator for PublishDocumentCommand
/// </summary>
public class PublishDocumentCommandValidator : AbstractValidator<PublishDocumentCommand>
{
    public PublishDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId is required");

        RuleFor(x => x.PublishedBy)
            .NotEmpty().WithMessage("PublishedBy is required");
    }
}

/// <summary>
/// Handler for PublishDocumentCommand
/// </summary>
public class PublishDocumentCommandHandler : IRequestHandler<PublishDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public PublishDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<DocumentDto> Handle(PublishDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get the document
        var documentId = DocumentId.From(request.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document == null)
            throw new InvalidOperationException($"Document with ID {request.DocumentId} not found");

        // Publish the document
        document.Publish(request.PublishedBy);

        // Save changes
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Map to DTO and return
        return _mapper.Map<DocumentDto>(document);
    }
}
