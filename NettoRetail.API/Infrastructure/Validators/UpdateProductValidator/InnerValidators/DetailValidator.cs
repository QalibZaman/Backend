using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class DetailValidator : AbstractValidator<DetailModel>
{
    public DetailValidator()
    {
        RuleFor((model) => model)
            .Must(HasAnyPropToUpdate)
            .WithMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);

        When((model) => model.IsDelete, () =>
        {
            RuleFor((model) => model.Id)
                .NotNull()
                .WithMessage("Id of the detail should be present when deleting a detail.");
        });

        When((model) => !model.IsDelete && !model.Id.HasValue && HasAnyPropToUpdate(model), () =>
        {
            RuleFor((model) => model.Name)
                .NotNull()
                .WithMessage("Name is required for a newly added value.");

            RuleFor((model) => model.TypeEnum)
                .NotNull()
                .WithMessage("Type is required for a newly added value");
        });

        When((model) => model.TypeEnum is not null, () =>
        {
            RuleFor((model) => model.Value)
                .NotNull()
                .WithMessage("Value shouldn't be null.");

            When((model) => model.TypeEnum is ProductDetailTypeEnum.Number, () =>
            {
                RuleFor((model) => model.Unit)
                    .NotNull()
                    .WithMessage("Unit should be present for the number data type.");

                RuleFor((model) => model.Value)
                    .Must((value) => decimal.TryParse(value, out _))
                    .WithMessage("Value should be a valid number.");
            }).Otherwise(() =>
            {
                RuleFor((model) => model.Unit)
                    .Null()
                    .WithMessage("Unit should be null for anything other than Number data type.");
            });

            When((model) => model.TypeEnum is ProductDetailTypeEnum.Color, () =>
            {
                RuleFor((model) => model.Value)
                    .Matches("^#[0-9A-Fa-f]{3}([0-9A-Fa-f]{3})?$")
                    .WithMessage("Value should be a valid hex value.");
            });

            When((model) => model.TypeEnum is ProductDetailTypeEnum.Logic, () =>
            {
                RuleFor((model) => model.Value)
                    .Must((value) => bool.TryParse(value, out _))
                    .WithMessage("Value should be a valid boolean.");
            });

            When((model) => model.TypeEnum is ProductDetailTypeEnum.String, () =>
            {
                RuleFor((model) => model.Value)
                    .Matches("^(?! )[A-Za-z0-9 ,._-]{1,50}(?<! )$")
                    .WithMessage("Invalid format for the value.");
            });
        }).Otherwise(() =>
        {
            RuleFor((model) => model.Unit)
                .Null()
                .WithMessage("Type should be present along with the unit.");

            RuleFor((model) => model.Value)
                .Null()
                .WithMessage("Type should be present along with the value.");
        });

        When((model) => model.Name is not null, () =>
        {
            RuleFor((model) => model.Name)
                .Matches("^[A-Za-z0-9_-]{2,50}$")
                .WithMessage("Invalid format for the name.");
        });
    }

    private static bool HasAnyPropToUpdate(DetailModel model)
    {
        return model.Name is not null ||
            model.TypeEnum is not null ||
            model.Unit is not null ||
            model.Value is not null ||
            model.OrderIndex is not null ||
            model.Id.HasValue && model.IsDelete;
    }
}