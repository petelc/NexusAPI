using AutoMapper;
using FluentValidation;
using MediatR;
using Ardalis.Result;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Documents.DTOs;

namespace Nexus.API.UseCases.Documents.Commands.UpdateDocument;

/// <summary>
/// Command to update an existing document
/// </summary>
public record UpdateDocumentCommand : IRequest<Result<UpdateDocumentResponse>>
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public Guid UpdatedBy { get; init; }
    public string? Status { get; init; }
}




