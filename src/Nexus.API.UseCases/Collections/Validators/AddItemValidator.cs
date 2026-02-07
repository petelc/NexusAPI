using FluentValidation;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Validators;

public class AddItemToCollectionValidator : AbstractValidator<AddItemToCollectionCommand>
{
  public AddItemToCollectionValidator()
  {
    RuleFor(x => x.CollectionId)
      .NotEmpty().WithMessage("CollectionId is required");

    RuleFor(x => x.ItemType)
      .NotEmpty().WithMessage("ItemType is required")
      .Must(BeValidItemType).WithMessage("ItemType must be one of: Document, Diagram, Snippet, SubCollection");

    RuleFor(x => x.ItemReferenceId)
      .NotEmpty().WithMessage("ItemReferenceId is required");
  }

  private bool BeValidItemType(string itemType)
  {
    var validTypes = new[] { "Document", "Diagram", "Snippet", "SubCollection" };
    return validTypes.Contains(itemType, StringComparer.OrdinalIgnoreCase);
  }
}
