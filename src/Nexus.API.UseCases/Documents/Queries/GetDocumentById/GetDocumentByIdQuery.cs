using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.Core.Aggregates.DocumentAggregate;
using Nexus.Core.Interfaces;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Queries.GetDocumentById;

/// <summary>
/// Query to get a document by its ID
/// </summary>
public record GetDocumentByIdQuery : IRequest<DocumentDto?>
{
    public Guid DocumentId { get; init; }
    public bool IncludeVersions { get; init; } = false;
}

/// <summary>
/// Validator for GetDocumentByIdQuery
/// </summary>
public class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
{
    public GetDocumentByIdQueryValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId is required");
    }
}

/// <summary>
/// Handler for GetDocumentByIdQuery
/// </summary>
public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public GetDocumentByIdQueryHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var documentId = DocumentId.From(request.DocumentId);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document == null)
            return null;

        var dto = _mapper.Map<DocumentDto>(document);

        // Optionally exclude versions to reduce payload size
        if (!request.IncludeVersions)
        {
            dto.Versions.Clear();
        }

        return dto;
    }
}
