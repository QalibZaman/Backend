using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class OrderStatusHistoryEntity
{
    public Guid Id { get; set; }
    public required int OrderId { get; set; }
    public required OrderStatusEnum OldStatus { get; set; }
    public required OrderStatusEnum NewStatus { get; set; }
    public required DateOnly UpdatedAt { get; set; }
    public OrderEntity Order { get; set; }
}
