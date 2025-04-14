using System;
using System.Linq;
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
        return ruleBuilder.Must(status =>
        {
            return Enum.TryParse(typeof(ProductDetailTypeEnum), status, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidProductDetailUnit<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(status =>
        {
            if (string.IsNullOrEmpty(status))
                return true;

            return Enum.TryParse(typeof(ProductDetailUnitEnum), status, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidUrl<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(url =>
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        });
    }

    public static IRuleBuilderOptions<T, string> MustHaveImageExtension<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(filename =>
        {
            string[] permittedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return permittedExtensions.Contains(extension);
        });
    }
}
