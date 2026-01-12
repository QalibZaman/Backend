using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record DeactivateShopCommand(int ShopId, string Reason, DateTime? DeactivationDate) : IRequest;

public sealed class DeactivateShopCommandHandler(
    IShopRepository shopRepository,
    IPublisher publisher) : IRequestHandler<DeactivateShopCommand>
{
    public async Task Handle(DeactivateShopCommand request, CancellationToken cancellationToken)
    {
        var deactivationDate = request.DeactivationDate ?? DateTime.UtcNow.AddDays(14);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException($"Shop with id {request.ShopId} not found");

        shop.Deactivate(request.Reason, deactivationDate);

        await shopRepository.UpdateAsync(shop, cancellationToken);

        await publisher.Publish(
            new DeactivateShopEventCommand(shop.Id, request.Reason, deactivationDate),
            cancellationToken);
    }
}
