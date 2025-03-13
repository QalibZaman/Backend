namespace NettoRetail.Application.Models.Category;

public class GetCategoriesResponse
{
    public ICollection<CategoryResponse> Categories { get; set; }
    public int TotalAmount { get; set; }
}
