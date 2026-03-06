namespace NettoRetail.Application.Models.InventoryItem
{
    public class InventoryItemDto
    {
        public required decimal Price { get; set; }
        public string ProductName { get; set; }
        public bool IsPromoted { get; set; }
        public string ProductUpc { get; set; } = string.Empty;
        public decimal? PromotionPrice { get; set; }
        public required int AvailableAmount { get; set; }
        public required int ReservedAmount { get; set; }
        public required int OnHandAmount { get; set; }
    }
}
