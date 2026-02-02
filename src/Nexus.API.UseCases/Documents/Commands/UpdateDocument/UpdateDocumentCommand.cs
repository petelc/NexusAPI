using AutoMapper;
using FluentValidation;
using MediatR;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Documents.Commands.UpdateDocument;

/// <summary>
/// Command to update an existing document
/// </summary>
public record UpdateDocumentCommand : IRequest<DocumentDto>
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public Guid UpdatedBy { get; init; }
}




