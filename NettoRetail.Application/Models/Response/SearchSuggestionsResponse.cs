namespace NettoRetail.Application.Models.Response;

public class SearchSuggestionsResponse
{
    public string Query { get; set; } = string.Empty;
    public PaginatedResponse<BrandResponse> Brands { get; set; } = new();
    public PaginatedResponse<CategoryResponse> Categories { get; set; } = new();
    public PaginatedResponse<InventoryItemListResponse> Products { get; set; } = new();
}
