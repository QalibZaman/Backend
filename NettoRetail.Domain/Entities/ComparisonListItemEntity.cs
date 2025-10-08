namespace NettoRetail.Domain.Entities
{
    public class ComparisonListItemEntity
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public decimal? PromotionPrice { get; set; }
        public string? Brand { get; set; }
        public bool IsPromoted { get; set; }
        public required int AvailableAmount { get; set; }
        public required int ReservedAmount { get; set; }
        public required int OnHandAmount { get; set; }
        public required string ProductLocation { get; set; }

        public Guid InventoryItemId { get; set; }
        public required InventoryItemEntity InventoryItem { get; set; }

        public int ComparisonListId { get; set; }
        public required ComparisonListEntity ComparisonList { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}