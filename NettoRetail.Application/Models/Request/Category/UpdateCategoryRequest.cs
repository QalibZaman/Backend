namespace NettoRetail.Application.Models.Request.Category
{
    public class UpdateCategoryRequest
    {
        public required string Name { get; set; }
        public int? ParentId { get; set; }
    }
}
