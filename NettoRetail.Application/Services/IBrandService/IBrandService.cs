using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.Services.IBrandService;

public interface IBrandService
{
    Task<PaginatedResponse<BrandResponse>> GetShopBrandsBySearchAsync(
         string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken);
}
