using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record DeleteLogoCommand(int ShopId) : IRequest;

public class DeleteLogoCommandHandler(IShopRepository shopRepository, IAzureBlobService blobService)
    : IRequestHandler<DeleteLogoCommand>
{
    public async Task Handle(DeleteLogoCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetShopByIdSpecification(request.ShopId);
        var shop = await shopRepository.GetAsync(specification, cancellationToken);
        if (shop is null) throw new NotFoundException("Shop not found.");
        if (shop.LogoUrl is null) throw new NotFoundException("Logo does not exist.");
        await blobService.DeleteByUrlAsync(
            shop.LogoUrl,
            cancellationToken);
        shop.LogoUrl = null;
        await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}