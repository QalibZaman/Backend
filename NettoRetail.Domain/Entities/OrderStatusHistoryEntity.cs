using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class OrderStatusHistoryEntity
{
    public Guid Id { get; set; }
    public required int OrderId { get; set; }
    public required OrderStatusEnum OldStatus { get; set; }
    public required OrderStatusEnum NewStatus { get; set; }
    public string? Comment { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public OrderEntity? Order { get; set; }
    public Guid? UpdatedById { get; set; }
    public StaffEntity? UpdatedBy { get; set; }
}
