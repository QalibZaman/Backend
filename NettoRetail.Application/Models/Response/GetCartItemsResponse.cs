namespace NettoRetail.Application.Models.Response;
public class GetCartItemsResponse
{
    public Guid CartId { get; set; }
    public List<GetCartItemResponse> Items { get; set; } = new();
    public int TotalQuantity { get; set; }
    public decimal TotalPrice { get; set; }
}
