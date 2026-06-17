namespace NettoRetail.Domain.Entities.JunctionEntities
{
    public class OrderItemEntity
    {
        public Guid Id { get; set; }
        public int OrderId { get; set; }
        public OrderEntity Order { get; set; } = null!;
        public int Quantity { get; set; }
        public InventoryItemEntity InventoryItem { get; set; } = null!;
        public Guid InventoryItemId { get; set; }
    }
}