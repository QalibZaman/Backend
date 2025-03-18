namespace NettoRetail.Application.Models.Response;

public class CategoryResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string URL { get; set; }
    public int Level { get; set; }
    public bool Visibility { get; set; }
    public int ItemsAmount { get; set; }
    public int? ParentId { get; set; }
}
