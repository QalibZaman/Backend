namespace NettoRetail.Application.Models.Response;
public class OrderItemResponse
{
    public required string ProductName { get; set; }
    public required string ProductUPC { get; set; }
    public required int AvailableAmount { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public bool IsOnSale { get; set; }
    public decimal Sale { get; set; } = 0.0m;
    public decimal Total { get;  set; }
    public Guid InventoryItemId { get; set; }
    public bool AlreadyReviewed { get; set; }
}
