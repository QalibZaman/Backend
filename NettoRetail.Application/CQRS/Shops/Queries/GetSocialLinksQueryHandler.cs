using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Link;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public class GetSocialLinksQueryHandler(
    IShopRepository shopRepository)
    : IRequestHandler<GetSocialLinksQuery, List<ShopLinkRequest>>
{
    public async Task<List<ShopLinkRequest>> Handle(
        GetSocialLinksQuery request,
        CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetAsync(
            new GetShopByIdWithLinksSpecification(request.ShopId),
            cancellationToken);

        if (shop is null)
        {
            throw new NotFoundException("Shop not found");
        }

        return shop.ShopLinks
            .Select(link => new ShopLinkRequest
            {
                SocialNetwork = link.SocialNetwork,
                Url = link.Url
            })
            .ToList();
    }
}