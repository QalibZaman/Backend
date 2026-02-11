namespace NettoRetail.Application.Models.Order;

public class OrderHistoryResponse
{
    public Guid Id { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string UpdatedBy { get; set; }
    public required string OrderStatus { get; set; }
    public string? Comment { get; set; }
}
