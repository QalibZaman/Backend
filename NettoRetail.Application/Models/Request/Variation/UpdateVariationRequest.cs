using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.Variation
{
    public class UpdateVariationRequest
    {
        public Guid InventoryItemId { get; set; }
        public long ProductUpc { get; set; }
        public bool IsActive { get; set; }
        public bool ApplyDescriptionFromParent { get; set; }
        public string? Description { get; set; }
        public bool ApplyPromotion { get; set; }
        public decimal Price { get; set; }
        public decimal? PromotionPrice { get; set; }
        public bool ApplyImagesFromParent { get; set; }
        public List<Guid>? ImageIds { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public string Status { get; set; }
        public int AvailableAmount { get; set; }
        public int ReservedAmount { get; set; }
        public int OnHandAmount { get; set; }
        public bool ApplyDetailsFromParent { get; set; }
        public List<CustomDetailModel>? CustomParameters { get; set; }
    }
}
