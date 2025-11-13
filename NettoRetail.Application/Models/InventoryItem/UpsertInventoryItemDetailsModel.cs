using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class UpsertInventoryItemDetailsModel
{
    public ICollection<UpsertProductDetailModel> Details { get; set; } = new List<UpsertProductDetailModel>();
    public ICollection<Guid> DetailsToDelete { get; set; } = [];
}
