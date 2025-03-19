namespace NettoRetail.Application.Models.Category;

public class AddCategoryRequest
{
    public required string Name { get; set; }
    public required string URL { get; set; }
    public required bool Visibility { get; set; }
    public int? ParentId { get; set; }
    public int ShopId { get; set; }
}
