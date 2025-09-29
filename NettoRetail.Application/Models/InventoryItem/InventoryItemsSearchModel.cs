using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Models.InventoryItem;

public class InventoryItemsSearchModel
{
    public string? UpcOrName { get; set; }
    public string? ProductName { get; set; }
    public string? ProductLocation { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public string? BrandName { get; set; }

    public decimal MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public int MinAvailableAmount { get; set; }
    public int? MaxAvailableAmount { get; set; }

    public int MinReservedAmount { get; set; }
    public int? MaxReservedAmount { get; set; }

    public int MinOnHandAmount { get; set; }
    public int? MaxOnHandAmount { get; set; }

    public InventoryItemSortingEnum? SortOption { get; set; }
    public List<SortingOption>? SortingOptions { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}