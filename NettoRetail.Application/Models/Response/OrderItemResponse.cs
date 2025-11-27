using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Application.Models.Response;
public class OrderItemResponse
{
    public string ProductName { get; set; }
    public string ProductUPC { get; set; }
    public int AvailableAmount { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsPromoted { get; set; }
    public decimal PromotionPrice { get; set; } = 0;
    public decimal Total { get; private set; }

    public void SetTotal()
    {
        if (IsPromoted) Total = PromotionPrice * Quantity;
        if (!IsPromoted) Total = Price * Quantity;
    }

    public static List<OrderItemResponse> GetListOfResponses(List<OrderItemEntity> orderItemEntities)
    {
        var result = new List<OrderItemResponse>();
        foreach (var orderItemEntity in orderItemEntities)
        {
            var response = GetOrderItemResponse(orderItemEntity);
            result.Add(response);
        }

        return result;
    }

    private static OrderItemResponse GetOrderItemResponse(OrderItemEntity itemEntity)
    {
        var item = new OrderItemResponse();
        item.ProductName = itemEntity.InventoryItem.ProductName;
        item.ProductUPC = itemEntity.InventoryItem.ProductUpc.ToString();
        item.AvailableAmount = itemEntity.InventoryItem.AvailableAmount;
        item.Price = itemEntity.InventoryItem.Price;
        item.Quantity = itemEntity.Quantity;
        item.IsPromoted = itemEntity.InventoryItem.IsPromoted;
        item.PromotionPrice = itemEntity.InventoryItem.PromotionPrice ?? 0;
        return item;
    }
}
