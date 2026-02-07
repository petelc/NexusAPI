using FluentValidation;
using Nexus.API.UseCases.Collections.Commands;

namespace Nexus.API.UseCases.Collections.Validators;

public class ReorderItemValidator : AbstractValidator<ReorderItemCommand>
{
  public ReorderItemValidator()
  {
    RuleFor(x => x.CollectionId)
      .NotEmpty().WithMessage("CollectionId is required");

    RuleFor(x => x.ItemReferenceId)
      .NotEmpty().WithMessage("ItemReferenceId is required");

    RuleFor(x => x.NewOrder)
      .GreaterThanOrEqualTo(0).WithMessage("NewOrder must be greater than or equal to 0");
  }
}
