using NettoRetail.Application.Models.Response.Category;

namespace NettoRetail.Application.Models.Response
{
    public class GetCategoriesSectionResponse
    {
        public int ShopId { get; set; }
        public bool Visibility { get; set; }
        public string Name { get; set; } = string.Empty;
        public IReadOnlyList<CategorySectionItemResponse> SelectedCategories { get; set; } = [];
        public int AvailableCategoriesCount { get; set; }
    }
}
