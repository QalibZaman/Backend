using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemDetailsModel
{
    public ICollection<ProductDetailModel> Details { get; set; } = new List<ProductDetailModel>();
    public ICollection<string> DetailsToDelete { get; set; } = new List<string>();
}
