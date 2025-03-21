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
}
