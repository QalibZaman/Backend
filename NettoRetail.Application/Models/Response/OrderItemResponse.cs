namespace NettoRetail.Application.Models.Response;
public class OrderItemResponse
{
    public required string ProductName { get; set; }
    public required string ProductUPC { get; set; }
    public required int AvailableAmount { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public decimal Total => Math.Round(Price * Quantity, 2);
}
