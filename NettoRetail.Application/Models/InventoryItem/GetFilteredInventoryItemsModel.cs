namespace NettoRetail.Application.Models.InventoryItem;

public class GetFilteredInventoryItemsModel
{
    public string? ProductName { get; set; }
    public string? ProductLocation { get; set; }
    public int CategoryId { get; set; }
    public int MaxAvailableAmount { get; set; }
    public int MinAvailableAmount { get; set; }
    public int MaxReservedAmount { get; set; }
    public int MinReservedAmount { get; set; }
    public int MaxOnHandAmount { get; set; }
    public int MinOnHandAmount { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal MinPrice { get; set; }
    public string? Status { get; set; }

    public List<SortingOption>? SortingOptions { get; set; }
}

public class SortingOption
{
    public string Field { get; set; } = string.Empty;
    public bool IsDescending { get; set; }
}