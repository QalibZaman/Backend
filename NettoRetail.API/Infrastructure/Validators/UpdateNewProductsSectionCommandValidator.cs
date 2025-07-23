using FluentValidation;
using NettoRetail.Application.CQRS.NewProducts.Command;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateNewProductsSectionCommandValidator : AbstractValidator<UpdateNewProductsSectionCommand>
{
  public UpdateNewProductsSectionCommandValidator()
  {
    RuleFor(x => x.Model.Name)
        .NotEmpty().WithMessage("Section name is required.")
        .MaximumLength(150);

    RuleFor(x => x.Model.InventoryItemIds)
        .NotNull().WithMessage("Products list cannot be null.")
        .Must(list => list.Any()).WithMessage("At least one product must be added.");
  }
}