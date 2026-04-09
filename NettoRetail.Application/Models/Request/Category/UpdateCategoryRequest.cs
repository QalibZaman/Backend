namespace NettoRetail.Application.Models.Request.Category;

public class UpdateCategoryRequest : ICategoryUpsertRequest
{
    public required string Name { get; set; }
    public string? URL { get; set; }
    public required bool Visibility { get; set; }
    public int? ParentId { get; set; }
}
