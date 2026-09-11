namespace NettoRetail.Application.Models.Request.Category;

public class AddCategoryRequest
{
    public required string Name { get; set; }
    public int? ParentId { get; set; }
    public bool Visibility { get; set; } = true;
}