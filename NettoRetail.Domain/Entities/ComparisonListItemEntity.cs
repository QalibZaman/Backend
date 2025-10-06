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

        public Guid InventoryItemId { get; set; } //FK to InventoryItem table
        public required InventoryItemEntity InventoryItem { get; set; } //Navigation

        public int ComparisonListId { get; set; } //FK to ComparisonList table
        public required ComparisonListEntity ComparisonList { get; set; } //Navigation property

        public DateTime CreatedAtUtc { get; set; }
    }
}