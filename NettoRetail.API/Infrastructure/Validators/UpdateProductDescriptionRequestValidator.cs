using FluentValidation;
using NettoRetail.Application.Models.Request.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;
public class UpdateProductDescriptionRequestValidator : AbstractValidator<UpdateProductsDescriptionRequest>
{
    public UpdateProductDescriptionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.");

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage("Description cannot exceed 5000 characters.");
    }
}
