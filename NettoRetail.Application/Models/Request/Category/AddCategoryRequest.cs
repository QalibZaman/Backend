using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Request.Category;

public class AddCategoryRequest : ICategoryUpsertRequest
{
    public required string Name { get; set; }

    [JsonIgnore]
    public string? URL { get; set; }

    public required bool Visibility { get; set; }
    public int? ParentId { get; set; }
}
