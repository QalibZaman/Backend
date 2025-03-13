using Mapster;
using MediatR;
using NettoRetail.Application.CQRS.Sessions.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public record GetShopByIdQuery(int ShopId) : IRequest<ShopResponse>;

public class GetShopByIdQueryHandler(IShopRepository shopRepository) : IRequestHandler<GetShopByIdQuery, ShopResponse>
{
    public async Task<ShopResponse> Handle(GetShopByIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetShopByIdSpecification(request.ShopId);

        var returnedShop = await shopRepository.GetAsync(specification, cancellationToken);

        var shopResponse = returnedShop.Adapt<ShopResponse>();

        return shopResponse;
    }
}
