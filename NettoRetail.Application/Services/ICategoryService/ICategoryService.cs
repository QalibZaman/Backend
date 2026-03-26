namespace NettoRetail.Application.Services.ICategoryService;

public interface ICategoryService
{
    Task DeleteCategoryAsync(int id, int shopId, CancellationToken cancellationToken = default);
}
