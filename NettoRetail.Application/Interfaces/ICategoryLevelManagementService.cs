using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface ICategoryLevelManagementService
{
    Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);
    Task UpdateCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);
    Task<PaginatedResponse<CategoryResponse>> GetShopCategoriesBySearchAsync(string query, int shopId, 
        PaginationRequest pageModel ,CancellationToken cancellationToken);
}
