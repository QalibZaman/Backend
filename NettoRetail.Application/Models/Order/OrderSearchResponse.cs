using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class OrderSearchResponse
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreationDate { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    public OrderDeliveryTypeEnum DeliveryType { get; set; }
}
