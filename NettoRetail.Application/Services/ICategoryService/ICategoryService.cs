namespace NettoRetail.Application.Services.ICategoryService;

public interface ICategoryService
{
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
