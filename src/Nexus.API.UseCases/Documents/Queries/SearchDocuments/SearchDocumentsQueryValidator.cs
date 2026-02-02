using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Documents.Queries.SearchDocuments;

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