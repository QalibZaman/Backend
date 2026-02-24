using System.Linq.Expressions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
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
            specBuilder.Add<GetOrderSearchSpecification>(searchTerm);
        }

        ApplySorting(specBuilder, model);

        specBuilder.AddInclude(x => x.Customer.Account);

        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        specBuilder.Skip(skip).Take(pagination.PageSize);

        return specBuilder.Build();
    }

    private static void ApplyCustomerFilters(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        builder.Add<GetOrdersByCustomerIdsSpecification>(model.CustomerIds);
        builder.Add<GetOrdersByFirstNamesSpecification>(model.CustomerFirstNames);
        builder.Add<GetOrdersByLastNamesSpecification>(model.CustomerLastNames);
        builder.Add<GetOrdersByEmailsSpecification>(model.CustomerEmails);
    }

    private static void ApplyOrderFilters(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        builder.Add<GetOrdersByOrderIdsSpecification>(model.OrderIds);
        builder.Add<GetOrdersByCreatorSpecification>(model.Creators);
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

          ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc)

          : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        var endDate = model.EndDate.HasValue

            ? DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc)

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
}
