using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemDetailsModel
{
    public ICollection<ProductDetailsModel> Details { get; set; } = new List<ProductDetailsModel>();
    public ICollection<string> DetailsToDelete { get; set; } = new List<string>();
}
