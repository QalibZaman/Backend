using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public record GetShopByIdQuery(int ShopId) : IRequest<ShopResponse>;

public class GetShopByIdQueryHandler(IShopRepository shopRepository, IMapper mapper) : IRequestHandler<GetShopByIdQuery, ShopResponse>
{
    public async Task<ShopResponse> Handle(GetShopByIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetShopByIdSpecification(request.ShopId);

        var returnedShop = await shopRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"There is no shop with such Id");

        var shopResponse = mapper.Map<ShopResponse>(returnedShop);

        return shopResponse;
    }
}
