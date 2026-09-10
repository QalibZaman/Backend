namespace NettoRetail.Application.Models.Response.Category;

public class CheckCategoryNameResponse
{
    public required bool IsAvailable { get; set; }
    public required string Message { get; set; }
}