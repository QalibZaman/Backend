namespace NettoRetail.Application.Models.Response;
public class OrderItemResponse
{
    public required string ProductName { get; set; }
    public required string ProductUPC { get; set; }
    public required int AvailableAmount { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public required bool IsPromoted { get; set; }
    public decimal PromotionPrice { get; set; } = 0.0m;
    public decimal Total { get;  set; }
}
