using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemDetailsModel
{
    public ICollection<UpdateProductDetailModel> Details { get; set; } = new List<UpdateProductDetailModel>();
    public ICollection<Guid>? DetailsToDelete { get; set; }
}
