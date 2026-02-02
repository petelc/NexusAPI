using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Documents.Queries.SearchDocuments;

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




