using FluentValidation;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators;

public class VariationCustomDetailModelValidator : AbstractValidator<CustomDetailModel>
{
    public VariationCustomDetailModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Custom parameter name is required.")
            .Matches("^[A-Za-z0-9_-]{2,50}$").WithMessage("Invalid format for the custom parameter name.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Custom parameter order index must be greater or equal to 0.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Custom parameter value is required.");

        When(x => x.TypeEnum == ProductDetailTypeEnum.Number, () =>
        {
            RuleFor(x => x.Unit)
                .NotNull().WithMessage("Unit should be present for the number data type.");

            RuleFor(x => x.Value)
                .Must(value => decimal.TryParse(value, out _))
                .WithMessage("Custom parameter value should be a valid number.");
        });

        When(x => x.TypeEnum != ProductDetailTypeEnum.Number, () =>
        {
            RuleFor(x => x.Unit)
                .Null().WithMessage("Unit should be null for anything other than Number data type.");
        });

        When(x => x.TypeEnum == ProductDetailTypeEnum.Color, () =>
        {
            RuleFor(x => x.Value)
                .Matches("^#[0-9A-Fa-f]{3}([0-9A-Fa-f]{3})?$")
                .WithMessage("Custom parameter value should be a valid hex value.");
        });

        When(x => x.TypeEnum == ProductDetailTypeEnum.Logic, () =>
        {
            RuleFor(x => x.Value)
                .Must(value => bool.TryParse(value, out _))
                .WithMessage("Custom parameter value should be a valid boolean.");
        });

        When(x => x.TypeEnum == ProductDetailTypeEnum.String, () =>
        {
            RuleFor(x => x.Value)
                .Matches("^(?! )[A-Za-z0-9 ,._-]{1,50}(?<! )$")
                .WithMessage("Invalid format for the custom parameter value.");
        });
    }
}
