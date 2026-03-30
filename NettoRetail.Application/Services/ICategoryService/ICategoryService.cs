namespace NettoRetail.Application.Services.ICategoryService;

public interface ICategoryService
{
    string BuildCategoryUrl(string categoryName, string? requestedUrl, string? parentUrl);
    Task DeleteCategoryAsync(int id, int shopId, CancellationToken cancellationToken = default);
}
