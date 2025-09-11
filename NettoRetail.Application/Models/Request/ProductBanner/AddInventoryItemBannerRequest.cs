using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request.ProductBanner
{
    public class AddInventoryItemBannerRequest
    {
        public required Guid InventoryItemId { get; set; }
        public required List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
