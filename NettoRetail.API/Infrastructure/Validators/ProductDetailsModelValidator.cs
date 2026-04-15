using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators;

public class ProductDetailsModelValidator : AbstractValidator<IInventoryItemDetailModel>
{
    public ProductDetailsModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Product detail name length must be at least 2 characters long.")
            .MaximumLength(50).WithMessage("Product detail name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Product detail name must only contain letters, digits, '-' and '_'.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Product detail index can't be less than 0");

        RuleFor(x => x.Type)
            .NotNull()
            .WithMessage("Type is required");

        RuleFor(x => x.Unit)
            .Must((model, unit) => unit == null
            || model.Type.Equals(ProductDetailTypeEnum.Number))
            .WithMessage("Unit can only be specified for numeric values.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required.")
            .Custom((value, context) =>
            {
                var typeEnum = context.InstanceToValidate.Type;

                switch (typeEnum)
                {
                    case ProductDetailTypeEnum.String:
                        if (value is not string strValue || !Regex.IsMatch(strValue, @"^(?! )[A-Za-z0-9.,\-_ ]{1,50}(?<! )$"))
                            context.AddFailure("Incorrect value.");
                        break;
                    case ProductDetailTypeEnum.Number:
                        if (value is not decimal numValue || !Regex.IsMatch(numValue.ToString(CultureInfo.InvariantCulture), @"^[0-9]+(\.[0-9]{1,2})$"))
                            context.AddFailure("Incorrect value. Must be a number");
                        break;
                    case ProductDetailTypeEnum.Color:
                        if (value is not string colorValue || !Regex.IsMatch(colorValue, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
                            context.AddFailure("Incorrect value.");
                        break;
                    case ProductDetailTypeEnum.Logic:
                        if (value is not bool)
                            context.AddFailure("Incorrect value.");
                        break;
                    default:
                        context.AddFailure("A valid Product type must be selected");
                        break;
                }
            });
    }
}
