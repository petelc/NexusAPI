using FluentValidation;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Validators;

public class UpdateCollectionValidator : AbstractValidator<UpdateCollectionCommand>
{
  public UpdateCollectionValidator()
  {
    RuleFor(x => x.CollectionId)
      .NotEmpty().WithMessage("CollectionId is required");

    RuleFor(x => x.Name)
      .MaximumLength(200).WithMessage("Collection name cannot exceed 200 characters")
      .When(x => !string.IsNullOrEmpty(x.Name));

    RuleFor(x => x.Description)
      .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
      .When(x => !string.IsNullOrEmpty(x.Description));

    RuleFor(x => x.Icon)
      .MaximumLength(50).WithMessage("Icon cannot exceed 50 characters")
      .When(x => !string.IsNullOrEmpty(x.Icon));

    RuleFor(x => x.Color)
      .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
      .WithMessage("Color must be a valid hex color code (e.g., #FF5733 or #F73)")
      .When(x => !string.IsNullOrEmpty(x.Color));
  }
}
