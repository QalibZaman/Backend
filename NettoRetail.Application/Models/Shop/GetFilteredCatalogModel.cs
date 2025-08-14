namespace NettoRetail.Application.Models.Shop;

public class GetFilteredCatalogModel
{
    public IEnumerable<string>? BrandsNames { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsPromoted { get; set; }
    public string? ProductName { get; set; }
    public string? ItemStatus { get; set; }
}