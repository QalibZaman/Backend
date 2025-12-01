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
    public decimal PromotionPrice { get; set; } = 0.0m;
    public decimal Total { get; private set; }

    public void SetTotal()
    {
        if (IsPromoted) Total = PromotionPrice * Quantity;
        if (!IsPromoted) Total = Price * Quantity;
    }
}
