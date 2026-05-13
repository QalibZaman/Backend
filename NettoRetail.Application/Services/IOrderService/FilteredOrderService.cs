using System.Linq.Expressions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IOrderService;

public class FilteredOrderService : IFilteredOrderService
{
    private static readonly Dictionary<string, Expression<Func<OrderEntity, object>>> SortExpressions = new()
    {
        { OrderSortingFieldConstants.CreationDate, o => o.CreationDate },
        { OrderSortingFieldConstants.OrderId, o => o.Id },
        { OrderSortingFieldConstants.OrderStatus, o => o.OrderStatus },
        { OrderSortingFieldConstants.DeliveryType, o => o.DeliveryType },
        { OrderSortingFieldConstants.Creator, o => o.Creator }
    };

    public Specification<OrderEntity> BuildFilteredOrdersSpecification(
        GetFilteredOrdersModel model,
        int shopId,
        string? searchTerm,
        PaginationRequest pagination)
    {
        var specBuilder = new SpecificationBuilder<OrderEntity>();

        specBuilder.Add<GetOrdersByShopIdSpecification>(shopId);

        ApplyCustomerFilters(specBuilder, model);

        ApplyOrderFilters(specBuilder, model);

        if (model.StartDate != null || model.EndDate != null)
            ApplyDateRangeFilter(specBuilder, model);

        ApplyEnumFilters(specBuilder, model);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            specBuilder.Add<GetOrderSearchSpecification>(searchTerm.Trim().ToLower());
        }

        ApplySorting(specBuilder, model);

        specBuilder.AddInclude(x => x.Customer);

        var skip = PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize);
        specBuilder.Skip(skip).Take(pagination.PageSize);

        return specBuilder.Build();
    }

    private static void ApplyCustomerFilters(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        if (model.CustomerIds?.Any() == true)
        {
            builder.Add<GetOrdersByCustomerIdsSpecification>(model.CustomerIds);
        }

        var firstNames = NormalizeTextFilters(model.CustomerFirstNames);
        if (firstNames.Count > 0)
        {
            builder.Add<GetOrdersByFirstNamesSpecification>(firstNames);
        }

        var lastNames = NormalizeTextFilters(model.CustomerLastNames);
        if (lastNames.Count > 0)
        {
            builder.Add<GetOrdersByLastNamesSpecification>(lastNames);
        }

        var emails = NormalizeTextFilters(model.CustomerEmails);
        if (emails.Count > 0)
        {
            builder.Add<GetOrdersByEmailsSpecification>(emails);
        }
    }

    private static void ApplyOrderFilters(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        if (model.OrderIds?.Any() == true)
        {
            builder.Add<GetOrdersByOrderIdsSpecification>(model.OrderIds);
        }

        var creators = NormalizeTextFilters(model.Creators);
        if (creators.Count > 0)
        {
            builder.Add<GetOrdersByCreatorSpecification>(creators);
        }
    }

    private static void ApplyEnumFilters(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        if (model.OrderStatuses?.Any() == true)
        {
            var enums = ParseEnums<OrderStatusEnum>(model.OrderStatuses);
            if (enums.Any()) builder.Add<GetOrdersByStatusEnumSpecification>(enums);
        }

        if (model.DeliveryTypes?.Any() == true)
        {
            var enums = ParseEnums<OrderDeliveryTypeEnum>(model.DeliveryTypes);
            if (enums.Any()) builder.Add<GetOrdersByDeliveryTypeEnumSpecification>(enums);
        }
    }

    private static void ApplyDateRangeFilter(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        var startDate = model.StartDate.HasValue
          ? DateTime.SpecifyKind(model.StartDate.Value.Date, DateTimeKind.Utc)
          : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        var endDate = model.EndDate.HasValue
            ? DateTime.SpecifyKind(model.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        builder.Add<GetOrdersByDateRangeSpecification>(startDate, endDate);
    }

    private static void ApplySorting(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        if (model.SortingOptions?.Any() == true)
        {
            foreach (var option in model.SortingOptions)
            {
                var expression = SortExpressions.TryGetValue(option.Field, out var exp)
                    ? exp
                    : SortExpressions[OrderSortingFieldConstants.CreationDate];

                builder.AddOrderBy(expression, option.IsDescending);
            }
        }
        else
        {
            builder.AddOrderBy(o => o.CreationDate, isDesc: true);
        }
    }

    private static List<TEnum> ParseEnums<TEnum>(List<string>? source)
        where TEnum : struct, Enum
    {
        if (source == null || !source.Any()) return new List<TEnum>();

        return source
            .Select(s => Enum.TryParse<TEnum>(s, true, out var parsed) ? (TEnum?)parsed : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();
    }

    private static List<string> NormalizeTextFilters(List<string>? source)
    {
        if (source == null || source.Count == 0)
        {
            return [];
        }

        return source
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();
    }
}
