namespace NettoRetail.Application.Models.InventoryItem;

public class PriceModel
{
    public decimal? Price { get; init; }
    public bool? IsPromoted { get; init; }
    public decimal? PromotionPrice { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}