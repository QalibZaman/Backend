namespace NettoRetail.Application.Models.Request.Category;

public interface ICategoryUpsertRequest
{
    string Name { get; set; }
    string? URL { get; set; }
    bool Visibility { get; set; }
    int? ParentId { get; set; }
}
