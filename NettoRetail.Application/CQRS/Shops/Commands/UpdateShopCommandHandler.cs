using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record UpdateShopCommand(int Id, ShopModel Model) : IRequest;

public class UpdateShopCommandHandler(IShopRepository shopRepository) : IRequestHandler<UpdateShopCommand>
{
    public async Task Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetShopByIdSpecification(request.Id);
        var shopToUpdate = await shopRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"There is no category with such Id");

        request.Model.Adapt(shopToUpdate);
        await shopRepository.UpdateAsync(shopToUpdate, cancellationToken);
    }
}
