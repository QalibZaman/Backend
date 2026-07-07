using System.Text.RegularExpressions;
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

    public static IRuleBuilderOptions<T, string?> MustBeAValidUrl<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(url =>
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                return false;

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                return false;

            var host = uriResult.Host;

            if (string.IsNullOrEmpty(host))
                return false;

            var dotIndex = host.LastIndexOf('.');
            if (dotIndex < 0 || dotIndex == host.Length - 1)
                return false;

            var tld = host.Substring(dotIndex + 1);
            if (tld.Length < 2)
                return false;

            return true;
        });
    }

    public static IRuleBuilderOptions<T, string> MustHaveImageExtension<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(filename =>
        {
            string[] permittedExtensions = [".jpg", ".jpeg", ".png"];
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return permittedExtensions.Contains(extension);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidOrderStatus<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(status =>
        {
            return string.IsNullOrWhiteSpace(status) || Enum.TryParse(typeof(OrderStatusEnum), status, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidAccountStatus<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(status =>
        {
            return string.IsNullOrWhiteSpace(status) || Enum.TryParse(typeof(StaffStatusEnum), status, true, out _);
        });
    }

    public static IRuleBuilderOptions<T, string?> MustBeAValidOrderDeliveryType<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(type =>
        {
            return string.IsNullOrWhiteSpace(type) || Enum.TryParse(typeof(OrderDeliveryTypeEnum), type, true, out _);
        });
    }
}
