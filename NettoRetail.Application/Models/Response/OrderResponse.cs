using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class OrderResponse
{
    public int Id { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public required string CustomerEmail { get; set; }
    public required DateTime CreationDate { get; set; }
    public required string DeliveryType { get; set; }
    public required string Creator { get; set; }
    public required string OrderStatus { get; set; }
    public required Guid CustomerId { get; set; }

    public required IEnumerable<OrderItemResponse> OrderItems { get; set; }
    public int TotalProducts { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalSale { get; set; }
    public decimal Total { get; set; }
}