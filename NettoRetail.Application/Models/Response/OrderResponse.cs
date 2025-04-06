using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class OrderResponse
{
    public int Id { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public required string CustomerEmail { get; set; }
    public required DateTime CreationDate { get; set; }
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public required string Creator { get; set; }
    public required OrderStatusEnum OrderStatus { get; set; }
    public Guid? CustomerId { get; set; }
}