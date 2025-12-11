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
            var enums = model.OrderStatuses
                .Select(s => (IsValid: Enum.TryParse<OrderStatusEnum>(s, true, out var parsed), Value: parsed))
                .Where(x => x.IsValid)
                .Select(x => x.Value)
                .ToList();

            if (enums.Any())
            {
                builder.Add<GetOrdersByStatusEnumSpecification>(enums);
            }
        }

        if (model.DeliveryTypes?.Any() == true)
        {
            var enums = model.DeliveryTypes
                .Select(t => (IsValid: Enum.TryParse<OrderDeliveryTypeEnum>(t, true, out var parsed), Value: parsed))
                .Where(x => x.IsValid)
                .Select(x => x.Value)
                .ToList();

            if (enums.Any())
            {
                builder.Add<GetOrdersByDeliveryTypeEnumSpecification>(enums);
            }
        }
    }

    private static void ApplyDateRangeFilter(SpecificationBuilder<OrderEntity> builder, GetFilteredOrdersModel model)
    {
        var startDate = model.StartDate.HasValue
            ? DateTime.SpecifyKind(model.StartDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var endDate = model.EndDate.HasValue
            ? DateTime.SpecifyKind(model.EndDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

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
}
