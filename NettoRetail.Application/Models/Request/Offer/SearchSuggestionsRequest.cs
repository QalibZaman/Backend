namespace NettoRetail.Application.Models.Request.Offer;

public class SearchSuggestionsRequest
{
    public string Query { get; set; } = string.Empty;
    public PaginationRequest? PageModel { get; set; }
}