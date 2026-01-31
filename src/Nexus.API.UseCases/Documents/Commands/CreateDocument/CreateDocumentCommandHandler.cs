using AutoMapper;
using MediatR;
using Nexus.Core.Aggregates.DocumentAggregate;
using Nexus.Core.Interfaces;
using Nexus.Core.ValueObjects;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Commands.CreateDocument;

/// <summary>
/// Handler for CreateDocumentCommand
/// </summary>
public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Create value objects
        var title = Title.Create(request.Title);
        var content = DocumentContent.Create(request.Content);

        // Create the document aggregate
        var document = Document.Create(
            title,
            content,
            request.CreatedBy,
            request.LanguageCode);

        // Add tags if provided
        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var tagName in request.Tags)
            {
                var tag = Tag.Create(tagName);
                document.AddTag(tag);
            }
        }

        // Save to repository
        var savedDocument = await _documentRepository.AddAsync(document, cancellationToken);

        // Map to DTO and return
        return _mapper.Map<DocumentDto>(savedDocument);
    }
}
