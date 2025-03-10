using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record UpdateShopCommand(int Id, ShopModel Model) : IRequest;

public class UpdateShopCommandHandler(IShopRepository shopRepository) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = request.Model.Adapt<ShopEntity>();
        shop.Id = request.Id;
        await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}
