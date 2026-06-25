using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetShopBrandsQuery(int ShopId, PaginationRequest Pagination) : IRequest<PaginatedResponse<BrandResponse>>;

public class GetShopBrandsQueryHandler(IBrandRepository brandRepository, IMapper mapper) : IRequestHandler<GetShopBrandsQuery, PaginatedResponse<BrandResponse>>
{
    public async Task<PaginatedResponse<BrandResponse>> Handle(GetShopBrandsQuery request, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(
    request.Pagination.PageNumber,
    request.Pagination.PageSize);

        var specification = new SpecificationBuilder<BrandEntity>()
            .Add<GetBrandsByShopIdSpecification>(request.ShopId)
            .AddOrderBy(b => b.Name)
            .AddOrderBy(b => b.Id)
            .Skip(skip)
            .Take(request.Pagination.PageSize)
            .Build();

        var (items, totalCount) =
            await brandRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var brands = mapper.Map<ICollection<BrandResponse>>(items);

        return new PaginatedResponse<BrandResponse>()
        {
            Items = brands,
            TotalAmount = totalCount,
        };
    }
}
