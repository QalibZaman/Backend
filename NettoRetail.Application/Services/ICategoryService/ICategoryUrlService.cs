namespace NettoRetail.Application.Services.ICategoryService;

public interface ICategoryUrlService
{
    Task<string> GenerateUniqueUrlAsync(string categoryName, int shopId, CancellationToken cancellationToken);
}