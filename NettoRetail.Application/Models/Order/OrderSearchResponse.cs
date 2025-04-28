using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class OrderSearchResponse
{
    public required int Id { get; set; }
    public required string CustomerEmail { get; set; }
    public required DateTime CreationDate { get; set; }
    public required OrderStatusEnum OrderStatus { get; set; }
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
}
