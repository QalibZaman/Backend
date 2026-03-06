using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateProductValidator : AbstractValidator<UpdateProductModel>
{
    private const int MAX_NUMBER_OF_DETAILS = 20;
    public UpdateProductValidator(
        IValidator<GeneralInfoModel> generalInfoValidator,
        IValidator<PriceModel> priceValidator,
        IValidator<DetailModel> detailValidator)
    {
        RuleFor((model) => model)
            .Must(HasAnyPropToUpdate)
            .WithMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);

        When((model) => model.GeneralInfo is not null, () =>
        {
            RuleFor((model) => model.GeneralInfo)
                .SetValidator(generalInfoValidator!);
        });

        When((model) => model.PriceInfo is not null, () =>
        {
            RuleFor((model) => model.PriceInfo)
                .SetValidator(priceValidator!);
        });

        When((model) => model.Details is not null, () =>
        {
            RuleFor((model) => model.Details)
                .Must((details) => details!.Count <= MAX_NUMBER_OF_DETAILS)
                .WithMessage($"The maximum number of details is {MAX_NUMBER_OF_DETAILS}.");

            RuleForEach((model) => model.Details)
                .SetValidator(detailValidator);
        });
    }

    private static bool HasAnyPropToUpdate(UpdateProductModel model)
    {
        return model.GeneralInfo is not null ||
            model.PriceInfo is not null ||
            model.Details is not null;
    }
}