using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.Core.Interfaces;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Queries.ListDocuments;

/// <summary>
/// Query to list documents with pagination and filtering
/// </summary>
public record ListDocumentsQuery : IRequest<ListDocumentsResult>
{
    public Guid? UserId { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; } = "CreatedAt";
    public string? SortOrder { get; init; } = "desc";
}

/// <summary>
/// Result for ListDocumentsQuery
/// </summary>
public record ListDocumentsResult
{
    public List<DocumentSummaryDto> Documents { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Validator for ListDocumentsQuery
/// </summary>
public class ListDocumentsQueryValidator : AbstractValidator<ListDocumentsQuery>
{
    public ListDocumentsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || 
                      new[] { "Title", "CreatedAt", "UpdatedAt", "Status" }.Contains(x))
            .WithMessage("Invalid SortBy value");

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrEmpty(x) || 
                      new[] { "asc", "desc" }.Contains(x?.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'");
    }
}

/// <summary>
/// Handler for ListDocumentsQuery
/// </summary>
public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public ListDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<ListDocumentsResult> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        // Get documents (in a real implementation, this would use specifications or filtered queries)
        IEnumerable<Core.Aggregates.DocumentAggregate.Document> documents;

        if (request.UserId.HasValue)
        {
            documents = await _documentRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else
        {
            // This would need a GetAllAsync method in the repository
            // For now, we'll throw an exception
            throw new NotImplementedException("GetAllAsync not yet implemented. Please provide a UserId filter.");
        }

        // Filter by status if provided
        if (!string.IsNullOrEmpty(request.Status))
        {
            documents = documents.Where(d => d.Status.ToString() == request.Status);
        }

        // Apply sorting
        documents = ApplySorting(documents, request.SortBy, request.SortOrder);

        var totalCount = documents.Count();

        // Apply pagination
        documents = documents
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        // Map to DTOs
        var documentDtos = _mapper.Map<List<DocumentSummaryDto>>(documents.ToList());

        return new ListDocumentsResult
        {
            Documents = documentDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    private static IEnumerable<Core.Aggregates.DocumentAggregate.Document> ApplySorting(
        IEnumerable<Core.Aggregates.DocumentAggregate.Document> documents,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "title" => isDescending
                ? documents.OrderByDescending(d => d.Title.Value)
                : documents.OrderBy(d => d.Title.Value),
            "updatedat" => isDescending
                ? documents.OrderByDescending(d => d.UpdatedAt)
                : documents.OrderBy(d => d.UpdatedAt),
            "status" => isDescending
                ? documents.OrderByDescending(d => d.Status)
                : documents.OrderBy(d => d.Status),
            _ => isDescending
                ? documents.OrderByDescending(d => d.CreatedAt)
                : documents.OrderBy(d => d.CreatedAt)
        };
    }
}
