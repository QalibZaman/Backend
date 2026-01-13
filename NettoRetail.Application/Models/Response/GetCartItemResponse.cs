namespace NettoRetail.Application.Models.Response;
public class GetCartItemResponse
{
    public long ProductUpc { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? PromotionPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubtotalPrice { get; set; }
}
