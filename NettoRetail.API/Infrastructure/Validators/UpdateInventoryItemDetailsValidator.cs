using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateInventoryItemDetailsValidator : AbstractValidator<UpdateInventoryItemDetailsModel>
{
    public UpdateInventoryItemDetailsValidator()
    {
        RuleFor(x => x.Details)
            .NotNull().WithMessage("Details list can't be null")
            .Must(HaveContinuousOrderIndexes).WithMessage("Order indexes must be continuous without missing numbers.");

        RuleForEach(x => x.Details).SetValidator(new ProductDetailsModelValidator());
    }

    private bool HaveContinuousOrderIndexes(ICollection<ProductDetailsModel> details)
    {
        if (details.Count == 0)
            return true;

        var orderedIndexes = details.Select(d => d.OrderIndex).OrderBy(x => x).ToList();
        return orderedIndexes.SequenceEqual(Enumerable.Range(orderedIndexes[0], orderedIndexes.Count));
    }
}
