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
        { OrderSortingFieldConstants.DeliveryType, o => o.Delivery != null ? o.Delivery.DeliveryType : null },
        { OrderSortingFieldConstants.Creator, o => o.Creator }
    };

    public Specification<OrderEntity> BuildFilteredOrdersSpecification(
        GetFilteredOrdersModel model,
        int shopId,
        string? searchTerm,
        PaginationRequest pagination)
    {
        var builder = new SpecificationBuilder<OrderEntity>();

        builder.Add<GetOrdersByShopIdSpecification>(shopId);
        builder.AddInclude(x => x.Customer);

        ApplyCustomerFilters(builder, model);
        ApplyOrderFilters(builder, model);
        ApplyDateFilter(builder, model.StartDate, model.EndDate);
        ApplyEnumFilters(builder, model);

        if (string.IsNullOrEmpty(searchTerm))
            builder.Add<GetOrderSearchSpecification>(searchTerm);

        ApplySorting(builder, model.SortingOptions);
        ApplyPaging(builder, pagination);

        return builder.Build();
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

    private static void ApplyDateFilter(SpecificationBuilder<OrderEntity> builder, DateTime? start, DateTime? end)
    {
        if (!start.HasValue && !end.HasValue)
            return;

        var from = start?.ToUniversalTime() ?? DateTime.MinValue.ToUniversalTime();
        var to = end?.ToUniversalTime() ?? DateTime.MaxValue.ToUniversalTime();

        builder.Add<GetOrdersByDateRangeSpecification>(from, to);
    }

    private static void ApplySorting(SpecificationBuilder<OrderEntity> builder, List<SortingOption>? options)
    {
        if (options == null || !options.Any())
        {
            builder.AddOrderBy(x => x.CreationDate, isDesc: true);
            return;
        }

        foreach (var option in options)
        {
            var expr = SortExpressions.GetValueOrDefault(option.Field, x => x.CreationDate);
            builder.AddOrderBy(expr, option.IsDescending);
        }
    }

    private static void ApplyPaging(SpecificationBuilder<OrderEntity> builder, PaginationRequest request)
    {
        var skip = PaginationHelper.CalculateSkip(request.PageNumber, request.PageSize);
        builder.Skip(skip).Take(request.PageSize);
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
