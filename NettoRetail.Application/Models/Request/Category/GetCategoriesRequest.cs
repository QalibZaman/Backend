using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;

namespace NettoRetail.Application.Models.Request.Category;

public class GetCategoriesRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = CategoryConstants.DefaultPageSize;
    public string? SearchTerm { get; set; }
    public CategorySortFieldEnum? SortField { get; set; }
    public SortDirectionEnum? SortDirection { get; set; }
}