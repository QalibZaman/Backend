using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemDetailsModel
{
    public ICollection<ProductDetailModel> Details { get; set; } = new List<ProductDetailModel>();
    public ICollection<Guid>? DetailsToDelete { get; set; }
}
