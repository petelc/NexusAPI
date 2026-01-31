using FluentValidation;

namespace Nexus.UseCases.Documents.Commands.CreateDocument;

/// <summary>
/// Validator for CreateDocumentCommand
/// </summary>
public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(1).WithMessage("Title must be at least 1 character")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");

        RuleFor(x => x.LanguageCode)
            .MaximumLength(10).WithMessage("Language code cannot exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.LanguageCode));

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Cannot have more than 20 tags")
            .When(x => x.Tags != null);

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .When(x => x.Tags != null);
    }
}
