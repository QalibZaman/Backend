using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class OrderEntity
{
    public int Id { get; set; }
    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public required string Creator { get; set; }
    public required OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.New;
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public required Guid CustomerId { get; set; }
    public int ShopId { get; set; }

    public ICollection<OrderItemEntity> OrderItems { get; set; } = null!;
    public CustomerEntity Customer { get; set; } = null!;
}