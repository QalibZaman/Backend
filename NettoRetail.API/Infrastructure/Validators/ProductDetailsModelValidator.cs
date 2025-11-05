using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators;

public class ProductDetailsModelValidator : AbstractValidator<ProductDetailModel>
{
    public ProductDetailsModelValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Product detail name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Product detail name must only contain letters, digits, '-' and '_'.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Product detail index can't be less than 0");

        RuleFor(x => x.Type)
            .MustBeAValidProductDetailType().WithMessage("Invalid type value provided.");

        RuleFor(x => x.Unit)
            .MustBeAValidProductDetailUnit().WithMessage("Invalid unit value provided.")
            .Must((model, unit) => string.IsNullOrEmpty(unit) || model.Type == ProductDetailTypeEnum.Number.ToString())
            .WithMessage("Unit can only be specified for numeric values.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required.")
            .Custom((value, context) =>
            {
                if (context.InstanceToValidate.Type == null)
                {
                    context.AddFailure("Type must be set before validating Value.");
                    return;
                }

                if (!Enum.TryParse<ProductDetailTypeEnum>(context.InstanceToValidate.Type, out var typeEnum))
                {
                    return;
                }

                switch (typeEnum)
                {
                    case ProductDetailTypeEnum.String:
                        if (value is not string)
                            context.AddFailure("Incorrect value.");
                        break;
                    case ProductDetailTypeEnum.Number:
                        if (value is not decimal)
                            context.AddFailure("Incorrect value.");
                        break;
                    case ProductDetailTypeEnum.Color:
                        if (value is not string colorValue || !Regex.IsMatch(colorValue, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
                            context.AddFailure("Incorrect value.");
                        break;
                    case ProductDetailTypeEnum.Logic:
                        if (value is not bool)
                            context.AddFailure("Incorrect value.");
                        break;
                }
            });
    }
}
