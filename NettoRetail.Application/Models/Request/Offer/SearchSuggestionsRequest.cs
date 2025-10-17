namespace NettoRetail.Application.Models.Request.Offer;

public class SearchSuggestionsRequest
{
    public int ShopId { get; set; }
    public string Query { get; set; } = string.Empty;
    public PaginationRequest? PageModel { get; set; }
}