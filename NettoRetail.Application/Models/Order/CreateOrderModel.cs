using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class CreateOrderModel
{
    public required Guid CustomerId { get; set; }
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public required IDictionary<Guid, int> Products { get; set; }
}