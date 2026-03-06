using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetShopBrandsQuery(int ShopId, PaginationRequest Pagination) : IRequest<PaginatedResponse<BrandResponse>>;

public class GetShopBrandsQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetShopBrandsQuery, PaginatedResponse<BrandResponse>>
{
    public async Task<PaginatedResponse<BrandResponse>> Handle(GetShopBrandsQuery request, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(request.Pagination.PageNumber, request.Pagination.PageSize);
        var specification = new GetBrandsByShopIdSpecificationWithPagination(
            request.ShopId,
            skip,
            request.Pagination.PageSize);

        var (items, totalCount) = await brandRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var brands = items.Adapt<ICollection<BrandResponse>>();

        return new PaginatedResponse<BrandResponse>()
        {
            Items = brands,
            TotalAmount = totalCount,
        };
    }
}
