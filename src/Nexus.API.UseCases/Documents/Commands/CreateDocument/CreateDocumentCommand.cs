using MediatR;
using Nexus.UseCases.Common.DTOs;

namespace Nexus.UseCases.Documents.Commands.CreateDocument;

/// <summary>
/// Command to create a new document
/// </summary>
public record CreateDocumentCommand : IRequest<DocumentDto>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CreatedBy { get; init; }
    public string? LanguageCode { get; init; }
    public List<string>? Tags { get; init; }
}
