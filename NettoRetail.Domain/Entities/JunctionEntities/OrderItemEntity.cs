namespace NettoRetail.Domain.Entities.JunctionEntities
{
    public class OrderItemEntity
    {
        public int OrderId { get; set; }
        public OrderEntity Order { get; set; } = null!;
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public InventoryItemEntity InventoryItem { get; set; } = null!;
    }
}