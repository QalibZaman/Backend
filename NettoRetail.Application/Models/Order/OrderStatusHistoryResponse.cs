namespace NettoRetail.Application.Models.Order;

public class OrderStatusHistoryResponse
{
    public Guid Id { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string UpdatedBy { get; set; }
    public required string Status { get; set; }
    public string? Comment { get; set; }
}
