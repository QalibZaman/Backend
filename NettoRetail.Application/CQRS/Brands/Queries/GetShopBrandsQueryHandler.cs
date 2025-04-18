using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetShopBrandsQuery(int ShopId) : IRequest<IEnumerable<BrandResponse>>;

public class GetShopBrandsQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetShopBrandsQuery, IEnumerable<BrandResponse>>
{
    public async Task<IEnumerable<BrandResponse>> Handle(GetShopBrandsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandsByShopIdSpecification(request.ShopId);
        var brands = await brandRepository.GetAllBySpecAsync(specification, cancellationToken);

        var brandsResponse = brands.Adapt<IEnumerable<BrandResponse>>();
        return brandsResponse;
    }
}
