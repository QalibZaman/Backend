using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Cart.Queries;

public record GetCartItemsQuery(Guid CustomerId) : IRequest<GetCartItemsResponse>;

public class GetCartItemQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartItemsQuery, GetCartItemsResponse>
{
    public async Task<GetCartItemsResponse> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartWithItemsAsync(request.CustomerId, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
        {
            return new GetCartItemsResponse
            {
                Items = new List<GetCartItemResponse>(),
                TotalQuantity = 0,
                TotalPrice = 0
            };
        }

        var items = cart.Items.Select(item => new GetCartItemResponse
        {
            InventoryItemId = item.InventoryItemId,
            ProductUpc = item.InventoryItem.ProductUpc,
            ProductName = item.InventoryItem.ProductName,
            Price = item.Price,
            PromotionPrice = item.PromotionPrice,
            Quantity = item.Quantity,
            ImageUrl = item.InventoryItem.Images.FirstOrDefault()?.BlobUrl,
            Status = item.InventoryItem.Status.ToString(),
            SubtotalPrice = (item.PromotionPrice ?? item.Price) * item.Quantity
        }).ToList();

        return new GetCartItemsResponse
        {
            CartId = cart.Id,
            Items = items,
            TotalQuantity = items.Sum(i => i.Quantity),
            TotalPrice = items.Sum(i => i.SubtotalPrice)
        };
    }
}