using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class PriceValidator : AbstractValidator<PriceModel>
{
    private const string PRICE_THRESHOLD_ERROR = "Price must be 0 or non-negative value.";
    public PriceValidator()
    {
        RuleFor((model) => model)
            .Must(HasAnyPropToUpdate)
            .WithMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);

        When((model) => model.Price is not null, () =>
        {
            RuleFor((model) => model.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage(PRICE_THRESHOLD_ERROR);
        });

        When(IsPromoted, () =>
        {
            RuleFor((model) => model.PromotionPrice)
                .NotNull()
                .WithMessage("Promotion price must be supplied when promotion is enabled.");

            RuleFor((model) => model.StartDate)
                .NotNull()
                .WithMessage("Start date must be provided when the promotion is enabled.");
        });

        When((model) => model.PromotionPrice is not null, () =>
        {
            RuleFor((model) => model.PromotionPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage(PRICE_THRESHOLD_ERROR);
        });

        When((model) => model.EndDate is not null, () =>
        {
            RuleFor((model) => model.EndDate)
                .GreaterThan((model) => model.StartDate)
                .WithMessage("End date must be greater than the start date.");
        });
    }

    private static bool HasAnyPropToUpdate(PriceModel model)
    {
        return model.Price is not null ||
            model.IsPromoted is not null ||
            model.PromotionPrice is not null ||
            model.StartDate is not null ||
            model.EndDate is not null;
    }

    private static bool IsPromoted(PriceModel model)
    {
        return model.IsPromoted.HasValue && model.IsPromoted.Value;
    }
}