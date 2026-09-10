namespace NettoRetail.Shared.Constants;

public static class CategoryConstants
{
    public const int DefaultPageSize = 25;

    public static readonly IReadOnlyCollection<int> AllowedPageSizes = [25, 50, 100];
}