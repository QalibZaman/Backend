namespace NettoRetail.Application.Models.Request.Category;

public class EditCategoriesSectionRequest
{
    public bool Visibility { get; set; }
    public string Name { get; set; }
    public IReadOnlyList<int> CategoryIds { get; set; } = [];
}
