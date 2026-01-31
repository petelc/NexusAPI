using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.Core.Interfaces;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Queries.SearchDocuments;

/// <summary>
/// Query to search documents by text
/// </summary>
public record SearchDocumentsQuery : IRequest<SearchDocumentsResult>
{
    public string SearchTerm { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Result for SearchDocumentsQuery
/// </summary>
public record SearchDocumentsResult
{
    public List<DocumentSummaryDto> Documents { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string SearchTerm { get; init; } = string.Empty;
}

/// <summary>
/// Validator for SearchDocumentsQuery
/// </summary>
public class SearchDocumentsQueryValidator : AbstractValidator<SearchDocumentsQuery>
{
    public SearchDocumentsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term is required")
            .MinimumLength(2).WithMessage("Search term must be at least 2 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");
    }
}

/// <summary>
/// Handler for SearchDocumentsQuery
/// </summary>
public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, SearchDocumentsResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public SearchDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<SearchDocumentsResult> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        // Search for documents
        var documents = await _documentRepository.SearchAsync(request.SearchTerm, cancellationToken);

        var totalCount = documents.Count();

        // Apply pagination
        documents = documents
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        // Map to DTOs
        var documentDtos = _mapper.Map<List<DocumentSummaryDto>>(documents.ToList());

        return new SearchDocumentsResult
        {
            Documents = documentDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm
        };
    }
}
