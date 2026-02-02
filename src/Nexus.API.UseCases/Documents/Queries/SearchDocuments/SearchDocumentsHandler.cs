using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Documents.Queries.SearchDocuments;

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