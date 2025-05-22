using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record ActivateShopCommand(int ShopId) : IRequest;

public class ActivateShopCommandHandler(IShopRepository shopRepository) : IRequestHandler<ActivateShopCommand>
{
    public async Task Handle(ActivateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");

        shop.IsActive = true;
        shop.DeactivationReason = null;
        shop.DeactivationDate = null;

        await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}
