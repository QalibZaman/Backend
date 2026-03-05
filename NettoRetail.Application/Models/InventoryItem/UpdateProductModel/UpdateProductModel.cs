namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateProductModel
{
    public GeneralInfoModel? GeneralInfo { get; init; }
    public PriceModel? PriceInfo { get; init; }
    public IList<DetailModel>? Details { get; init; }
}