using NettoRetail.Application.Models.Response.Category;

namespace NettoRetail.Application.Models.Response;

public class GetCategoriesResponse
{
    public required ICollection<CategoryResponse> Categories { get; set; }
    public int TotalAmount { get; set; }
}