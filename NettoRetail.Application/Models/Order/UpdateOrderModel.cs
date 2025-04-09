using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class UpdateOrderModel
{
    public required Guid CustomerId { get; set; }
    public OrderDeliveryTypeEnum? DeliveryType { get; set; }
    public required IDictionary<Guid, int> ItemsToUpsert { get; set; }
    public required IEnumerable<Guid> ItemsToDelete { get; set; }
}