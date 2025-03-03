namespace NettoRetail.Application.Models.Category;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string URL { get; set; }
    public int Level { get; set; }
    public bool Visibility { get; set; }
    public int? ParentId { get; set; }
}
