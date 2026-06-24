using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsQuery(int OrderId, int UserShopId) : IRequest<OrderProductsResponse>;

public class GetOrderProductsQueryHandler(
    IOrderRepository orderRepository,
    IProductReviewRepository productReviewRepository,
    IMapper mapper)
    : IRequestHandler<GetOrderProductsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(
        GetOrderProductsQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new GetOrderByIdAndShopIdSpecification(
            request.OrderId,
            request.UserShopId);

        var order = await orderRepository.GetOrderWithInventoryItemsAsync(
            spec,
            cancellationToken)
            ?? throw new NotFoundException(
                HandlerConstants.Order.OrderNotFoundMessage);

        var response = mapper.Map<OrderProductsResponse>(order);

        foreach (var item in response.OrderItems)
        {
            var reviewSpec = new HasCustomerReviewedProductSpecification(
                order.CustomerId,
                item.InventoryItemId);

            item.AlreadyReviewed =
                await productReviewRepository.Exists(
                    reviewSpec,
                    cancellationToken);

            item.IsVerifiedPurchase =
                order.OrderStatus == OrderStatusEnum.Done;
        }

        return response;
    }
}
