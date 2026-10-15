using MediatR;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands
{
    public class AddSocialLinksCommandHandler(
     IShopLinkRepository shopLinkRepository)
     : IRequestHandler<AddSocialLinksCommand>
    {
        public async Task Handle(
            AddSocialLinksCommand request,
            CancellationToken cancellationToken)
        {
            var shopLinks = request.Request.Links
                .Select(link => new ShopLinkEntity
                {
                    ShopId = request.ShopId,
                    SocialNetwork = link.SocialNetwork,
                    Url = link.Url.Trim()
                })
                .ToList();

            await shopLinkRepository.AddRangeAsync(
                shopLinks,
                cancellationToken);
        }
    }
}
