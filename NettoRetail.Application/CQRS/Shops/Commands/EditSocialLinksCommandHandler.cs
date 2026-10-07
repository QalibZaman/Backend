using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands
{
    public class EditSocialLinksCommandHandler(
    IShopRepository shopRepository)
    : IRequestHandler<EditSocialLinksCommand>
    {
        public async Task Handle(
            EditSocialLinksCommand request,
            CancellationToken cancellationToken)
        {
            var shop = await shopRepository.GetAsync(
                new GetShopByIdWithLinksSpecification(request.ShopId),
                cancellationToken);

            if (shop is null)
            {
                throw new NotFoundException("Shop not found");
            }

            var requestedLinks = request.Request.Links;

            var requestedNetworks = requestedLinks
                .Select(x => x.SocialNetwork)
                .ToHashSet();

            var linksToRemove = shop.ShopLinks
                .Where(x => !requestedNetworks.Contains(x.SocialNetwork))
                .ToList();

            foreach (var link in linksToRemove)
            {
                shop.ShopLinks.Remove(link);
            }

            foreach (var requestedLink in requestedLinks)
            {
                var existingLink = shop.ShopLinks
                    .FirstOrDefault(x =>
                        x.SocialNetwork == requestedLink.SocialNetwork);

                if (existingLink is not null)
                {
                    existingLink.Url = requestedLink.Url.Trim();
                }
                else
                {
                    shop.ShopLinks.Add(new ShopLinkEntity
                    {
                        ShopId = shop.Id,
                        SocialNetwork = requestedLink.SocialNetwork,
                        Url = requestedLink.Url.Trim()
                    });
                }
            }

            await shopRepository.CommitAsync(cancellationToken);
        }
    }
}
