using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class CreateOrderModel
{
    public Guid? CustomerId { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public required string CustomerEmail { get; set; }
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public required IDictionary<Guid, int> Products { get; set; }
}