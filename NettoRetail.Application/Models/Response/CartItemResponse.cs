namespace NettoRetail.Application.Models.Response;

public class CartItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductUpc { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public decimal? PromotionPrice { get; set; }
}
