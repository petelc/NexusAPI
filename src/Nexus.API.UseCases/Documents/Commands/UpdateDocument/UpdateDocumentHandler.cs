using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Documents.Commands.UpdateDocument;

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
        // if (!string.IsNullOrEmpty(request.Title))
        // {
        //     var newTitle = DocumentTitle.Create(request.Title);
        //     document.UpdateTitle(newTitle, request.UpdatedBy);
        // }

        // Save changes
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Map to DTO and return
        return _mapper.Map<DocumentDto>(document);
    }
}