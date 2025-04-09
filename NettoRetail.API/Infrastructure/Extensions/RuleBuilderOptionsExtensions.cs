using FluentValidation;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeAValidInventoryItemStatus<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(status =>
        {
            if (string.IsNullOrEmpty(status))
                return true;

            return Enum.TryParse(typeof(InventoryItemStatusEnum), status, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidProductDetailType<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(type =>
        {
            return Enum.TryParse(typeof(ProductDetailTypeEnum), type, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidProductDetailUnit<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(unit =>
        {
            if (string.IsNullOrEmpty(unit))
                return true;

            return Enum.TryParse(typeof(ProductDetailUnitEnum), unit, true, out _);
        });
    }
    public static IRuleBuilderOptions<T, string?> MustBeAValidDeliveryType<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(type =>
        {
            if (string.IsNullOrEmpty(type))
                return true;

            return Enum.TryParse(typeof(OrderDeliveryTypeEnum), type, true, out _);
        });
    }
}
