using FluentValidation;

namespace Nexus.API.UseCases.Documents.Create;

/// <summary>
/// Validator for CreateDocumentCommand
/// </summary>
public class CreateDocumentValidator : AbstractValidator<CreateDocumentCommand>
{
  public CreateDocumentValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Title is required")
      .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

    RuleFor(x => x.Content)
      .NotEmpty().WithMessage("Content is required");

    RuleFor(x => x.Status)
      .Must(status => string.IsNullOrEmpty(status) || 
                      new[] { "draft", "published", "archived" }.Contains(status.ToLower()))
      .WithMessage("Status must be one of: draft, published, archived");

    RuleForEach(x => x.Tags)
      .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");
  }
}
