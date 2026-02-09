namespace NettoRetail.Application.Models.Request.Category;

public class EditCategoriesSectionRequest
{
    public int ShopId { get; set; }
    public bool Visibility { get; set; }
    public string Name { get; set; }
    public int NumberOfCategories { get; set; }
}