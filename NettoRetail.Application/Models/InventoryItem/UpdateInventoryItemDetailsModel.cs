using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemDetailsModel
{
    public ICollection<ProductDetailsModel> Details { get; set; } = new List<ProductDetailsModel>();
    public ICollection<Guid> DetailsToDelete { get; set; } = new List<Guid>();
}
