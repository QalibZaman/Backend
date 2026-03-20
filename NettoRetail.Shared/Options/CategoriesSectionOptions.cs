namespace NettoRetail.Shared.Options;

public class CategoriesSectionOptions
{
    public const int DefaultSelectionCount = 6;
    public const int MaxSelectionCount = 8;

    public int DefaultSelectionCountValue { get; set; } = DefaultSelectionCount;
    public int MaxSelectionCountValue { get; set; } = MaxSelectionCount;

    public string AtLeastOneCategoryErrorMessage { get; set; } = "At least one category must be selected.";
    public string MaxSelectionErrorMessage { get; set; } = $"Maximum {MaxSelectionCount} categories allowed. Please remove one to add another.";
    public string DuplicateCategoriesErrorMessage { get; set; } = "Duplicate categories are not allowed.";
    public string InvalidCategoryIdsErrorMessage { get; set; } = "Selected categories must be level 1, visible, and belong to your shop.";
}
