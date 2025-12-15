using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetShopBrandsQuery(int ShopId, PaginationRequest Pagination) : IRequest<PaginatedResponse<BrandResponse>>;

public class GetShopBrandsQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetShopBrandsQuery, PaginatedResponse<BrandResponse>>
{
    public async Task<PaginatedResponse<BrandResponse>> Handle(GetShopBrandsQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<BrandEntity>();
        var skip = (request.Pagination.PageNumber - 1) * request.Pagination.PageSize;

        specificationBuilder.Add<GetBrandsByShopIdSpecification>(request.ShopId);

        var specification = specificationBuilder.Skip(skip).Take(request.Pagination.PageSize).Build();

        //.Add<GetBrandsByShopIdSpecification>(request.ShopId)
        //.Skip(skip)
        //.Take(request.Pagination.PageSize)
        //.Build();

        var ( items, totalCount ) = await brandRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var brands = items.Adapt<ICollection<BrandResponse>>();

        return new PaginatedResponse<BrandResponse>()
        {
            Items = brands,
            TotalAmount = totalCount,
        };
    }
}
