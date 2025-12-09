using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class OrderProductsResponse
{
    public decimal Sale { get; set; } = 0.0m;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string DeliveryType { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    public ICollection<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}
