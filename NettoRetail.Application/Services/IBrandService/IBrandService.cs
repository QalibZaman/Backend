using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IBrandService;

public interface IBrandService
{
    Task<PaginatedResponse<BrandResponse>> GetShopBrandsBySearchAsync(
         string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken);
    Task<BrandEntity> GetBrandByNameWithCacheAsync(
           Dictionary<string, BrandEntity> cache,
           string brandName,
           int shopId,
           CancellationToken cancellationToken);

    Task EnsureBrandCanBeCreatedAsync(int shopId, string name, string? websiteUrl, CancellationToken cancellationToken);
}
